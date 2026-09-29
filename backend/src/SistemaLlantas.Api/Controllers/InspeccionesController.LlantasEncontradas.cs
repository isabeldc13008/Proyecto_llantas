using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Inspecciones;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Application.Operaciones;

namespace SistemaLlantas.Api.Controllers;

public sealed partial class InspeccionesController
{
    public sealed class RegistrarLlantaEncontradaDto
    {
        public Guid PosicionId { get; init; }
        public Guid? LlantaAnteriorId { get; init; }
        [Required, StringLength(50)] public string Codigo { get; init; } = "";
        [Required, StringLength(100)] public string Serial { get; init; } = "";
        public Guid MarcaId { get; init; }
        public Guid ReferenciaId { get; init; }
        public Guid DimensionId { get; init; }
        public Guid TipoLlantaId { get; init; }
        public Guid CentroId { get; init; }
        [Range(0,100)] public decimal ProfundidadInicial { get; init; }
        [Required, StringLength(400)] public string Motivo { get; init; } = "";
        public bool DiscrepanciaFisica { get; init; }
        public void Validar()
        {
            if(!DiscrepanciaFisica) throw new ValidacionException("Confirma que la llanta ya está físicamente instalada. Un montaje operativo requiere Programación o Montajes.");
            if(string.IsNullOrWhiteSpace(Codigo)||Codigo.Length>50||string.IsNullOrWhiteSpace(Serial)||Serial.Length>100
                ||string.IsNullOrWhiteSpace(Motivo)||Motivo.Length>400||ProfundidadInicial is <0 or >100
                ||new[]{PosicionId,MarcaId,ReferenciaId,DimensionId,TipoLlantaId,CentroId}.Contains(Guid.Empty))
                throw new ValidacionException("Completa código, serial, catálogos, posición, centro y motivo válidos. La profundidad inicial debe estar entre 0 y 100 mm.");
        }
    }
    public sealed record ReferenciaEncontradaDto(Guid Id,string Codigo,string Nombre,Guid MarcaId);
    public sealed record CatalogosLlantaEncontradaDto(IReadOnlyList<OpcionInspeccionDto> Marcas,IReadOnlyList<ReferenciaEncontradaDto> Referencias,IReadOnlyList<OpcionInspeccionDto> Dimensiones,IReadOnlyList<OpcionInspeccionDto> Tipos);

    [HttpGet("{id:guid}/llantas-encontradas/opciones"),Authorize(Policy="Inspecciones.Crear")]
    public async Task<CatalogosLlantaEncontradaDto> CatalogosLlantaEncontrada(Guid id,CancellationToken ct)
    {
        if(!await InspeccionAsignable(id).AnyAsync(ct))throw new UnauthorizedAccessException("La inspección no te pertenece, está finalizada o fuera de alcance.");
        return new(
            await db.Marcas.AsNoTracking().Where(x=>x.Activo).OrderBy(x=>x.Nombre).Select(x=>new OpcionInspeccionDto(x.Id,x.Codigo,x.Nombre)).ToListAsync(ct),
            await db.Referencias.AsNoTracking().Where(x=>x.Activo&&x.Marca.Activo).OrderBy(x=>x.Nombre).Select(x=>new ReferenciaEncontradaDto(x.Id,x.Codigo,x.Nombre,x.MarcaId)).ToListAsync(ct),
            await db.Dimensiones.AsNoTracking().Where(x=>x.Activo).OrderBy(x=>x.Nombre).Select(x=>new OpcionInspeccionDto(x.Id,x.Codigo,x.Nombre)).ToListAsync(ct),
            await db.TiposLlanta.AsNoTracking().Where(x=>x.Activo).OrderBy(x=>x.Nombre).Select(x=>new OpcionInspeccionDto(x.Id,x.Codigo,x.Nombre)).ToListAsync(ct));
    }

    [HttpPost("{id:guid}/llantas-encontradas"),Authorize(Policy="Inspecciones.Crear")]
    public async Task<ActionResult<ContextoInspeccionDto>> RegistrarLlantaEncontrada(Guid id,RegistrarLlantaEncontradaDto dto,
        [FromServices]ILlantaService llantas,[FromServices]IOperacionService operaciones,
        [FromServices]ICicloVidaLlantaService ciclo,CancellationToken ct)
    {
        dto.Validar();
        Guid vehiculoId=Guid.Empty;
        try
        {
            await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>
            {
                db.ChangeTracker.Clear();
                await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
                var inspeccion=await InspeccionAsignable(id).Include(x=>x.Vehiculo).SingleOrDefaultAsync(ct)
                    ??throw new UnauthorizedAccessException("La inspección no te pertenece, está finalizada o fuera de alcance.");
                vehiculoId=inspeccion.VehiculoId;
                if(dto.CentroId!=inspeccion.Vehiculo.CentroId||dto.CentroId!=inspeccion.CentroId)
                    throw new ValidacionException("El centro físico debe ser el centro autorizado de la inspección y del vehículo.");
                if(!await db.InspeccionesDetalle.AnyAsync(d=>d.InspeccionId==id&&d.PosicionVehiculoId==dto.PosicionId
                    &&d.PosicionVehiculo.EjeVehiculo.VehiculoId==vehiculoId&&d.LlantaId==dto.LlantaAnteriorId
                    &&d.PosicionVehiculo.LlantaActualId==dto.LlantaAnteriorId,ct))
                    throw new ConflictoException("La posición cambió o no pertenece a la inspección. Actualiza antes de corregir.");
                if(!await db.Marcas.AnyAsync(x=>x.Id==dto.MarcaId&&x.Activo,ct)
                    ||!await db.Referencias.AnyAsync(x=>x.Id==dto.ReferenciaId&&x.MarcaId==dto.MarcaId&&x.Activo,ct)
                    ||!await db.Dimensiones.AnyAsync(x=>x.Id==dto.DimensionId&&x.Activo,ct)
                    ||!await db.TiposLlanta.AnyAsync(x=>x.Id==dto.TipoLlantaId&&x.Activo,ct)
                    ||!await db.Centros.AnyAsync(x=>x.Id==dto.CentroId&&x.Activo,ct))
                    throw new ValidacionException("Los catálogos deben estar activos y la referencia debe pertenecer a la marca.");
                var estado=await db.EstadosLlanta.Where(x=>x.Activo&&x.PermiteMontaje&&!x.EsDisposicionFinal&&(x.Codigo=="DISPONIBLE"||x.Codigo=="DIS"))
                    .OrderByDescending(x=>x.Codigo=="DISPONIBLE").Select(x=>(Guid?)x.Id).FirstOrDefaultAsync(ct)
                    ??throw new ConflictoException("No hay estado disponible activo configurado para registrar la llanta.");
                var motivo=$"Llanta registrada durante inspección física · {id}. {dto.Motivo.Trim()}";
                var nueva=await llantas.CrearAsync(new(){Codigo=dto.Codigo.Trim(),Serial=dto.Serial.Trim(),MarcaId=dto.MarcaId,ReferenciaId=dto.ReferenciaId,
                    DimensionId=dto.DimensionId,TipoLlantaId=dto.TipoLlantaId,CentroId=dto.CentroId,EstadoLlantaId=estado,
                    UbicacionActual="Inventario",ProfundidadInicial=dto.ProfundidadInicial,Observaciones=motivo},Usuario(),User.AlcanceCentros(),ct);
                await CorregirLlantaFisicaAsync(inspeccion,dto.PosicionId,new(nueva.Id,dto.Motivo,true,nueva.Serial,dto.LlantaAnteriorId),operaciones,ciclo,ct);
                await db.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });
        }
        catch(DbUpdateException e) when(e.InnerException is SqlException {Number:2601 or 2627})
        { throw new ConflictoException("Ya existe una llanta con el código o serial indicado. Busca la llanta existente."); }
        return Created(string.Empty,await service.ObtenerContextoAsync(vehiculoId,User.AlcanceCentros(),ct));
    }
}
