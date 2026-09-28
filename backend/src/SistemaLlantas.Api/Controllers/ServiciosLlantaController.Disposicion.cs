using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Api.Controllers;

public sealed partial class ServiciosLlantaController
{
    public sealed record EvaluarDisposicionDto(bool Reutilizable,string Observaciones);

    [HttpPost("{id:guid}/evaluar-disposicion"),Authorize(Policy="ServiciosLlanta.Opcionar")]
    public Task<OrdenDto> EvaluarDisposicion(Guid id,EvaluarDisposicionDto dto,CancellationToken ct)
    {
        if(dto is null||string.IsNullOrWhiteSpace(dto.Observaciones)||dto.Observaciones.Length>1000)
            throw new ValidacionException("El concepto técnico es obligatorio y admite máximo 1000 caracteres.");
        return CambiarDisposicion(id,async x=>
        {
            if(x.Estado!="PENDIENTE_EVALUACION_TECNICA")throw new ConflictoException("La orden ya fue evaluada o no está pendiente de evaluación técnica.");
            if(dto.Reutilizable)
            {
                await ValidarDesmontada(x,ct);
                await operaciones.MoverAsync(new(){LlantaId=x.LlantaId,TipoDestino="Inventario",Motivo=$"Evaluación técnica reutilizable · orden {x.Id}",Observaciones=dto.Observaciones.Trim()},User.Username(),User.AlcanceCentros(),ct);
            }
            x.Resultado=dto.Reutilizable?"REUTILIZABLE":"DISPOSICION";
            x.Estado=dto.Reutilizable?"RETORNADA_INVENTARIO":"PENDIENTE_APROBACION";
            x.Observaciones=dto.Observaciones.Trim();x.EvaluadoPor=User.Username();x.FechaEvaluacion=DateTimeOffset.UtcNow;
        },ct);
    }

    private async Task<bool> EsDisposicion(Guid id,CancellationToken ct)
        =>(await Orden(id,User.AlcanceCentros(),ct)).Tipo==TipoServicioLlanta.DisposicionFinal;

    private Task<OrdenDto> EnviarDisposicion(Guid id,CancellationToken ct)=>throw new ValidacionException("Selecciona las órdenes aprobadas y crea un lote con destino explícito.");
    private Task<OrdenDto> RecibirDisposicion(Guid id,CancellationToken ct)=>throw new ValidacionException("Registra la recepción seleccionando las llantas del lote de disposición.");
    private Task<OrdenDto> CerrarDisposicion(Guid id,CerrarOrdenDto dto,CancellationToken ct)=>throw new ValidacionException("Confirma la disposición de las llantas seleccionadas desde su lote.");

    private async Task ValidarDesmontada(OrdenServicioLlanta x,CancellationToken ct)
    {
        if(await db.AsignacionesLlantaPosicion.AnyAsync(a=>a.LlantaId==x.LlantaId&&a.EsActiva,ct)||await db.PosicionesVehiculo.AnyAsync(p=>p.LlantaActualId==x.LlantaId,ct))
            throw new ConflictoException("La llanta continúa montada. Debe desmontarse primero.");
    }

    private Task<OrdenDto> CambiarDisposicion(Guid id,Func<OrdenServicioLlanta,Task> cambio,CancellationToken ct)
        =>db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>
        {
            db.ChangeTracker.Clear();
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var x=await Orden(id,User.AlcanceCentros(),ct);
            if(x.Tipo!=TipoServicioLlanta.DisposicionFinal)throw new ValidacionException("La orden no corresponde a disposición final.");
            await cambio(x);
            x.UsuarioModificacion=User.Username();x.FechaModificacion=DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            var result=await Dto(id,User.AlcanceCentros(),ct);
            await tx.CommitAsync(ct);
            return result;
        });
}
