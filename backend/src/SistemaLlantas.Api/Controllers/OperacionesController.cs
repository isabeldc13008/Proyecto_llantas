using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Operaciones;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Api.Security;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Application.Llantas;
using Application = SistemaLlantas.Application;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Authorize]
public sealed partial class OperacionesController(IOperacionService service,ICicloVidaLlantaService ciclo,LlantasDbContext db):ControllerBase
{
    [HttpGet("api/mis-actividades"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<IReadOnlyList<ActividadDto>> Actividades(CancellationToken ct)=>service.MisActividadesAsync(Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/actividades/{id:guid}/iniciar"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<ActividadDto> Iniciar(Guid id,CancellationToken ct)=>service.IniciarActividadAsync(id,Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/actividades/{id:guid}/completar"),Authorize(Policy="Actividades.ConsultarPropias")]
    public Task<ActividadDto> Completar(Guid id,CancellationToken ct)=>service.CompletarActividadAsync(id,Usuario(),User.AlcanceCentros(),ct);
    [HttpPost("api/movimientos"),Authorize(Policy="Operaciones.Ejecutar")]
    public Task<MovimientoDto> Mover(EjecutarMovimientoDto dto,CancellationToken ct)
    {
        throw new ValidacionException("Envía la operación manual mediante una solicitud para autorización.");

    }
    [HttpPost("api/desmontajes"),Authorize(Policy="Operaciones.Ejecutar")]
    public Task<MovimientoDto> Desmontar(DesmontarLlantaDto dto,CancellationToken ct)=>throw new ValidacionException("Envía el desmontaje mediante una solicitud para autorización.");
    [HttpGet("api/operaciones/solicitudes"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<IReadOnlyList<SolicitudOperacionDto>> Solicitudes(CancellationToken ct){var a=User.AlcanceCentros();return await db.SolicitudesOperacion.AsNoTracking().Where(x=>x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)||(x.CentroDestinoId.HasValue&&a.CentroIds.Contains(x.CentroDestinoId.Value)))).OrderByDescending(x=>x.FechaCreacion).Select(x=>new SolicitudOperacionDto(x.Id,x.Tipo,x.Estado.ToString(),x.CentroId,x.Centro.Nombre,x.LlantaId,x.Llanta.Codigo,x.PosicionOrigenId,x.PosicionDestinoId,x.TipoDestino,x.CentroDestinoId,x.Motivo,x.Observaciones,x.Solicitante,x.Aprobador,x.MotivoRechazo,x.FechaCreacion,x.FechaRecepcionDestino,Convert.ToBase64String(x.RowVersion))).ToListAsync(ct);}
    [HttpGet("api/operaciones/movimientos"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<Pagina<MovimientoTrazabilidadDto>> Movimientos([FromQuery] ConsultaMovimientos consulta,CancellationToken ct)
    {
        var a=User.AlcanceCentros();
        var page=Math.Max(1,consulta.Pagina);var size=Math.Clamp(consulta.Tamano,1,100);
        var q=db.MovimientosDetalle.AsNoTracking().Where(x=>x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.Movimiento.CentroId)));
        if(!string.IsNullOrWhiteSpace(consulta.Buscar)){var term=consulta.Buscar.Trim();q=q.Where(x=>x.Llanta.Codigo.Contains(term)||x.Llanta.Serial.Contains(term)||x.Movimiento.Numero.Contains(term)||db.PosicionesVehiculo.Any(p=>(p.Id==x.PosicionOrigenId||p.Id==x.PosicionDestinoId)&&(p.EjeVehiculo.Vehiculo.Placa.Contains(term)||p.EjeVehiculo.Vehiculo.NumeroInterno.Contains(term))));}
        if(!string.IsNullOrWhiteSpace(consulta.Numero))q=q.Where(x=>x.Movimiento.Numero.Contains(consulta.Numero.Trim()));
        if(!string.IsNullOrWhiteSpace(consulta.Tipo))q=q.Where(x=>x.Movimiento.Tipo==consulta.Tipo);
        if(consulta.CentroId.HasValue){if(!a.Autoriza(consulta.CentroId.Value))throw new UnauthorizedAccessException("Centro no autorizado.");q=q.Where(x=>x.Movimiento.CentroId==consulta.CentroId);}
        if(!string.IsNullOrWhiteSpace(consulta.Usuario))q=q.Where(x=>x.Movimiento.Usuario.Contains(consulta.Usuario.Trim()));
        if(consulta.Desde.HasValue)q=q.Where(x=>x.Movimiento.FechaCreacion>=consulta.Desde);
        if(consulta.Hasta.HasValue){var exclusive=consulta.Hasta.Value.Date.AddDays(1);q=q.Where(x=>x.Movimiento.FechaCreacion<exclusive);}
        if(!string.IsNullOrWhiteSpace(consulta.Origen)){var term=consulta.Origen.Trim();q=q.Where(x=>x.PosicionOrigenId.HasValue&&db.PosicionesVehiculo.Any(p=>p.Id==x.PosicionOrigenId&&(p.Codigo.Contains(term)||p.EjeVehiculo.Vehiculo.Placa.Contains(term)||p.EjeVehiculo.Vehiculo.NumeroInterno.Contains(term))));}
        if(!string.IsNullOrWhiteSpace(consulta.Destino)){var term=consulta.Destino.Trim();q=q.Where(x=>x.DestinoDescripcion!.Contains(term)||(x.PosicionDestinoId.HasValue&&db.PosicionesVehiculo.Any(p=>p.Id==x.PosicionDestinoId&&(p.Codigo.Contains(term)||p.EjeVehiculo.Vehiculo.Placa.Contains(term)||p.EjeVehiculo.Vehiculo.NumeroInterno.Contains(term)))));}
        var total=await q.CountAsync(ct);
        var items=await q.OrderByDescending(x=>x.Movimiento.FechaCreacion).ThenBy(x=>x.Movimiento.Numero).Skip((page-1)*size).Take(size)
            .Select(x=>new MovimientoTrazabilidadDto(x.MovimientoId,x.Movimiento.Numero,x.Movimiento.FechaCreacion,x.Movimiento.Tipo,x.LlantaId,x.Llanta.Codigo,x.Llanta.Serial,
                x.PosicionOrigenId.HasValue?db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionOrigenId).Select(p=>p.EjeVehiculo.Vehiculo.Placa+" / "+p.Codigo).FirstOrDefault()??"—":x.CentroDestinoId.HasValue?x.Movimiento.Centro.Nombre:x.PosicionDestinoId.HasValue?"Inventario":"—",
                x.PosicionDestinoId.HasValue?db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionDestinoId).Select(p=>p.EjeVehiculo.Vehiculo.Placa+" / "+p.Codigo).FirstOrDefault()??x.TipoDestino.ToString():x.CentroDestinoId.HasValue?db.Centros.Where(c=>c.Id==x.CentroDestinoId).Select(c=>c.Nombre).FirstOrDefault()??x.TipoDestino.ToString():x.DestinoDescripcion??x.TipoDestino.ToString(),
                db.PosicionesVehiculo.Where(p=>p.Id==(x.PosicionDestinoId??x.PosicionOrigenId)).Select(p=>p.EjeVehiculo.Vehiculo.NumeroInterno+" · "+p.EjeVehiculo.Vehiculo.Placa+" / "+p.Codigo).FirstOrDefault()??"—",x.Movimiento.Centro.Nombre,
                db.SolicitudesOperacion.Where(s=>s.MovimientoEjecutadoId==x.MovimientoId).Select(s=>s.KilometrajeVehiculo).FirstOrDefault()??db.AsignacionesLlantaPosicion.Where(s=>s.MovimientoOrigenId==x.MovimientoId&&s.LlantaId==x.LlantaId).Select(s=>s.KilometrajeMontaje).FirstOrDefault(),
                db.AsignacionesLlantaPosicion.Where(s=>s.MovimientoOrigenId==x.MovimientoId&&s.LlantaId==x.LlantaId).Select(s=>s.KilometrajeRecorrido).FirstOrDefault(),x.Movimiento.Usuario,
                db.SolicitudesOperacion.Where(s=>s.MovimientoEjecutadoId==x.MovimientoId).Select(s=>s.ActividadProgramadaId).FirstOrDefault(),x.Movimiento.Motivo,x.Movimiento.Observaciones,"EJECUTADO",
                db.PosicionesVehiculo.Where(p=>p.Id==(x.PosicionDestinoId??x.PosicionOrigenId)).Select(p=>p.EjeVehiculo.Vehiculo.NumeroInterno).FirstOrDefault(),
                db.PosicionesVehiculo.Where(p=>p.Id==(x.PosicionDestinoId??x.PosicionOrigenId)).Select(p=>p.EjeVehiculo.Vehiculo.Placa).FirstOrDefault(),
                db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionOrigenId).Select(p=>p.Codigo).FirstOrDefault(),
                db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionDestinoId).Select(p=>p.Codigo).FirstOrDefault(),
                x.Movimiento.Centro.Nombre,
                x.CentroDestinoId.HasValue?db.Centros.Where(c=>c.Id==x.CentroDestinoId).Select(c=>c.Nombre).FirstOrDefault():x.Movimiento.Centro.Nombre,
                db.SolicitudesOperacion.Where(s=>s.MovimientoEjecutadoId==x.MovimientoId).Select(s=>(Guid?)s.Id).FirstOrDefault())).ToListAsync(ct);
        return new(items,page,size,total);
    }
    [HttpPost("api/operaciones/solicitudes"),Authorize(Policy="Operaciones.Solicitar")]
    public async Task<ActionResult<SolicitudOperacionDto>> Solicitar(CrearSolicitudOperacionDto dto,CancellationToken ct)
    {
        if(dto is null||string.IsNullOrWhiteSpace(dto.Tipo))throw new ValidacionException("El tipo de operación es obligatorio.");
        if(dto.Tipo.Length>50||(dto.TipoDestino?.Length??0)>50)throw new ValidacionException("El tipo de operación y destino admiten máximo 50 caracteres.");
        if(string.IsNullOrWhiteSpace(dto.Motivo)||dto.Motivo.Length>500)throw new ValidacionException("El motivo es obligatorio y admite máximo 500 caracteres.");
        if((dto.Observaciones?.Length??0)>1000)throw new ValidacionException("Las observaciones admiten máximo 1000 caracteres.");
        if(dto.KilometrajeVehiculo<0||dto.KilometrajeVehiculo>9999999999999999.99m)throw new ValidacionException("El kilometraje debe ser mayor o igual a cero y admitir como máximo 16 dígitos enteros.");
        if(dto.Asignaciones is not null&&dto.Asignaciones.Any(x=>x is null))throw new ValidacionException("Cada asignación debe contener una posición y una llanta válidas.");
        if(dto.Asignaciones is not null)return await SolicitarJuego(dto,ct);
        if(dto.Tipo.Equals("Reemplazar llanta",StringComparison.OrdinalIgnoreCase)||dto.Tipo.Equals("Cambio de juego",StringComparison.OrdinalIgnoreCase))throw new ValidacionException("La operación requiere sus asignaciones.");
        if(dto.LlantaId==Guid.Empty)throw new ValidacionException("Selecciona una llanta válida.");
        if(dto.PosicionOrigenId==Guid.Empty||dto.PosicionDestinoId==Guid.Empty)throw new ValidacionException("Selecciona una posición válida.");
        if(dto.Tipo.Equals("Montaje",StringComparison.OrdinalIgnoreCase)&&!dto.PosicionDestinoId.HasValue)throw new ValidacionException("El montaje requiere una posición destino.");
        if(!(dto.PosicionDestinoId.HasValue&&!dto.PosicionOrigenId.HasValue)&&!dto.CentroDestinoId.HasValue&&string.IsNullOrWhiteSpace(dto.TipoDestino))throw new ValidacionException("El tipo de destino es obligatorio.");
        if(dto.ActividadProgramadaId.HasValue&&await db.SolicitudesOperacion.AnyAsync(s=>s.ActividadProgramadaId==dto.ActividadProgramadaId&&s.GrupoOperacionId.HasValue,ct))throw new ValidacionException("Ejecuta las llantas asignadas desde la programación; no se permite sustituirlas.");
        if(dto.Tipo.Contains("rot",StringComparison.OrdinalIgnoreCase)&&(dto.CentroDestinoId.HasValue||dto.TipoDestino!="Posicion"||(!string.IsNullOrEmpty(dto.DestinoDesplazada)&&dto.DestinoDesplazada!="Posicion")))throw new ValidacionException("La rotación solo cambia posiciones dentro del mismo vehículo.");
        if(dto.Tipo.Contains("rot",StringComparison.OrdinalIgnoreCase)&&(!dto.PosicionOrigenId.HasValue||!dto.PosicionDestinoId.HasValue||dto.PosicionOrigenId==dto.PosicionDestinoId))throw new ValidacionException("Selecciona origen ocupado y otra posición del mismo vehículo.");
        var mounting=dto.PosicionDestinoId.HasValue&&!dto.PosicionOrigenId.HasValue;
        if((mounting||dto.Tipo.Equals("Montaje",StringComparison.OrdinalIgnoreCase))&&!User.HasClaim("permiso","operaciones.montar"))return Forbid();
        if((dto.Tipo.Equals("Montaje",StringComparison.OrdinalIgnoreCase)&&!mounting)||(mounting&&(dto.CentroDestinoId.HasValue||dto.LlantaDesplazadaId.HasValue)))throw new ValidacionException("El montaje requiere una posición libre y no permite desplazamientos.");
        if((dto.PosicionOrigenId.HasValue||dto.PosicionDestinoId.HasValue)&&!dto.KilometrajeVehiculo.HasValue)throw new ValidacionException("Ingresa el kilometraje actual.");
        var a=User.AlcanceCentros();Guid id=Guid.Empty;
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            db.ChangeTracker.Clear();
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var tire=await db.Llantas.SingleOrDefaultAsync(x=>x.Id==dto.LlantaId&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)),ct)??throw new KeyNotFoundException("Llanta no encontrada.");
            if(dto.CentroDestinoId.HasValue&&(!a.Autoriza(dto.CentroDestinoId.Value)||dto.CentroDestinoId==tire.CentroId))throw new UnauthorizedAccessException("Centro destino no autorizado o inválido.");

            var scheduled=false;
            if(dto.ActividadProgramadaId.HasValue)
            {
                if(!dto.Tipo.Equals("Montaje",StringComparison.OrdinalIgnoreCase))throw new ValidacionException("Esta operación manual requiere autorización; no admite ejecución programada en esta fase.");
                if(await db.SolicitudesOperacion.AnyAsync(x=>x.ActividadProgramadaId==dto.ActividadProgramadaId&&x.Estado==EstadoSolicitudOperacion.EJECUTADO,ct))throw new ConflictoException("La programación ya fue ejecutada.");
                var activity=await db.ActividadesProgramadas.Include(x=>x.TecnicoUsuario).SingleOrDefaultAsync(x=>x.Id==dto.ActividadProgramadaId&&x.Activo,ct);
                var vehicleId=await db.PosicionesVehiculo.Where(x=>x.Id==(dto.PosicionDestinoId??dto.PosicionOrigenId)).Select(x=>(Guid?)x.EjeVehiculo.VehiculoId).SingleOrDefaultAsync(ct);
                scheduled=activity is not null && activity.CentroId==tire.CentroId && activity.VehiculoId==vehicleId && vehicleId.HasValue
                    && activity.Estado is EstadoActividad.Pendiente or EstadoActividad.EnEjecucion
                    && string.Equals(activity.TipoActividad,dto.Tipo,StringComparison.OrdinalIgnoreCase)
                    && activity.LlantaId==dto.LlantaId
                    && activity.PosicionVehiculoId==(dto.PosicionDestinoId??dto.PosicionOrigenId)
                    && (activity.TecnicoId==Usuario()||activity.TecnicoId==Usuario()+".local"||activity.TecnicoUsuario?.Username==Usuario());

                if(!scheduled)throw new ValidacionException("La programación no es válida para esta operación, vehículo, posición o técnico.");
                if(await db.SolicitudesOperacion.AnyAsync(x=>x.ActividadProgramadaId==dto.ActividadProgramadaId&&x.Estado==EstadoSolicitudOperacion.EJECUTADO,ct))throw new ConflictoException("La programación ya fue ejecutada.");
            }
            if(mounting&&!scheduled)await service.ValidarMontajeAsync(dto.LlantaId,dto.PosicionDestinoId!.Value,dto.KilometrajeVehiculo,a,ct);
            var item=new SolicitudOperacion{Tipo=mounting?"Montaje":dto.Tipo,Estado=scheduled?EstadoSolicitudOperacion.APROBADO:EstadoSolicitudOperacion.PENDIENTE_APROBACION,CentroId=tire.CentroId,LlantaId=tire.Id,PosicionOrigenId=dto.PosicionOrigenId,PosicionDestinoId=dto.PosicionDestinoId,TipoDestino=mounting?"Posicion":dto.CentroDestinoId.HasValue?"Traslado":dto.TipoDestino!,CentroDestinoId=dto.CentroDestinoId,LlantaDesplazadaId=dto.LlantaDesplazadaId,PosicionDestinoDesplazadaId=dto.PosicionDestinoDesplazadaId,DestinoDesplazada=dto.DestinoDesplazada,Motivo=dto.Motivo.Trim(),Observaciones=dto.Observaciones,KilometrajeVehiculo=dto.KilometrajeVehiculo,ActividadProgramadaId=dto.ActividadProgramadaId,Solicitante=Usuario(),Aprobador=scheduled?"Programación autorizada":null,FechaDecision=scheduled?DateTimeOffset.UtcNow:null,UsuarioCreacion=Usuario()};
            db.SolicitudesOperacion.Add(item);await db.SaveChangesAsync(ct);
            if(scheduled)await Ejecutar(item,a,ct);
            await tx.CommitAsync(ct);id=item.Id;
        });
        return Created(string.Empty,await ObtenerSolicitud(id,a,ct));
    }
    [HttpPost("api/operaciones/solicitudes/{id:guid}/resolver"),Authorize(Policy="Operaciones.Aprobar")]
    public async Task<SolicitudOperacionDto> Resolver(Guid id,ResolverSolicitudDto dto,CancellationToken ct)
    {
        var a=User.AlcanceCentros();
        return await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>
        {
            db.ChangeTracker.Clear();
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            // Query the request itself: tire filters must not hide its existence or decision.
            var item=await db.SolicitudesOperacion.SingleOrDefaultAsync(x=>x.Id==id,ct);
            if(item is null||!item.Activo)throw new SolicitudNoEncontradaException();
            if(!a.Autoriza(item.CentroId))throw new UnauthorizedAccessException("La solicitud está fuera de tus centros autorizados.");
            if(item.Estado!=EstadoSolicitudOperacion.PENDIENTE_APROBACION)throw new ConflictoException($"La solicitud ya fue procesada: {item.Estado}. Actualiza Autorizaciones.");
            if(item.GrupoOperacionId.HasValue){await ResolverGrupo(item,dto,a,ct);var groupedResponse=await MapSolicitudAsync(item,ct);await tx.CommitAsync(ct);return groupedResponse;}
            if(item.Solicitante==Usuario()&&!User.HasClaim("permiso","operaciones.aprobar_propia"))throw new UnauthorizedAccessException("No puede resolver su propia solicitud.");
            item.Aprobador=Usuario();item.FechaDecision=DateTimeOffset.UtcNow;
            if(!dto.Aprobar)
            {
                if(string.IsNullOrWhiteSpace(dto.Motivo)||dto.Motivo.Length>500)throw new ValidacionException("El motivo de rechazo es obligatorio (máximo 500 caracteres).");
                item.Estado=EstadoSolicitudOperacion.RECHAZADO;item.MotivoRechazo=dto.Motivo.Trim();await db.SaveChangesAsync(ct);
            }
            else
            {
                item.Estado=EstadoSolicitudOperacion.APROBADO;
                try{await Ejecutar(item,a,ct);}
                catch(KeyNotFoundException ex){throw new ConflictoException($"La operación ya no puede ejecutarse: {ex.Message} Actualiza la llanta y la posición.");}
            }
            var response=await MapSolicitudAsync(item,ct);
            await tx.CommitAsync(ct);
            return response;
        });
    }
    [HttpPost("api/operaciones/solicitudes/{id:guid}/recibir"),Authorize(Policy="Operaciones.Aprobar")]
    public async Task<SolicitudOperacionDto> Recibir(Guid id,CancellationToken ct)
    {
        var a=User.AlcanceCentros();
        await db.Database.CreateExecutionStrategy().ExecuteAsync(async()=>{
            db.ChangeTracker.Clear();
            await using var tx=await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable,ct);
            var item=await db.SolicitudesOperacion.Include(x=>x.Llanta).ThenInclude(x=>x.EstadoLlanta).SingleOrDefaultAsync(x=>x.Id==id&&x.Activo&&x.Estado==EstadoSolicitudOperacion.EJECUTADO&&x.CentroDestinoId.HasValue&&(a.VerTodos||a.CentroIds.Contains(x.CentroDestinoId.Value)),ct)??throw new KeyNotFoundException("Traslado pendiente de recepción no encontrado.");
            if(item.FechaRecepcionDestino.HasValue)throw new ConflictoException("El traslado ya fue recibido.");
            if(item.Llanta.EstadoLlanta.Codigo!="EN_TRASLADO"||item.Llanta.CentroId!=item.CentroDestinoId||await db.AsignacionesLlantaPosicion.AnyAsync(x=>x.LlantaId==item.LlantaId&&x.EsActiva,ct))throw new ConflictoException("La llanta ya no está desmontada en el centro destino.");
            var state=await db.EstadosLlanta.Where(x=>x.Activo&&(x.Codigo=="DISPONIBLE"||x.Codigo=="DIS")).OrderByDescending(x=>x.Codigo=="DISPONIBLE").FirstOrDefaultAsync(ct)??throw new ValidacionException("No está configurado el estado DISPONIBLE.");
            item.FechaRecepcionDestino=DateTimeOffset.UtcNow;item.Llanta.UbicacionActual="Inventario";item.Llanta.EstadoLlanta=state;item.Llanta.EstadoLlantaId=state.Id;item.Llanta.UsuarioModificacion=Usuario();item.UsuarioModificacion=Usuario();
            await db.SaveChangesAsync(ct);await tx.CommitAsync(ct);
        });
        return await ObtenerSolicitud(id,a,ct);
    }
    [HttpGet("api/operaciones/vehiculos")]
    public Task<Pagina<SistemaLlantas.Application.Vehiculos.VehiculoResumenDto>> VehiculosMontaje([FromQuery]ConsultaPaginada consulta,[FromServices]SistemaLlantas.Application.Vehiculos.IVehiculoService vehiculos,CancellationToken ct)=>PuedePlanificarMontaje()?vehiculos.ConsultarAsync(consulta,User.AlcanceCentros(),ct):throw new UnauthorizedAccessException();
    [HttpGet("api/operaciones/vehiculos/{id:guid}")]
    public async Task<IActionResult> VehiculoMontaje(Guid id,[FromServices]SistemaLlantas.Application.Vehiculos.IVehiculoService vehiculos,CancellationToken ct)
        =>!PuedePlanificarMontaje()?Forbid():await vehiculos.ObtenerAsync(id,User.AlcanceCentros(),ct) is {} vehicle?Ok(vehicle):NotFound();
    [HttpGet("api/operaciones/llantas-disponibles")]
    public async Task<IActionResult> Disponibles([FromQuery]Guid vehiculoId,[FromQuery]string? buscar,CancellationToken ct)
    {
        if(!PuedePlanificarMontaje())return Forbid();
        var a=User.AlcanceCentros();
        var centro=await db.Vehiculos.Where(x=>x.Id==vehiculoId&&x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.CentroId))).Select(x=>(Guid?)x.CentroId).SingleOrDefaultAsync(ct);
        if(!centro.HasValue)return NotFound();
        var q=SistemaLlantas.Infrastructure.Services.LlantasDisponibles.Consulta(db).Where(x=>x.CentroId==centro);
        if(!string.IsNullOrWhiteSpace(buscar)){var term=buscar.Trim();q=q.Where(x=>x.Codigo.Contains(term)||x.Serial.Contains(term)||x.Marca.Nombre.Contains(term)||x.Referencia.Nombre.Contains(term)||x.Dimension.Nombre.Contains(term));}
        return Ok(await q.OrderBy(x=>x.Codigo).Take(50).Select(x=>new{x.Id,x.Codigo,x.Serial,Marca=x.Marca.Nombre,Referencia=x.Referencia.Nombre,Dimension=x.Dimension.Nombre,Estado=x.EstadoLlanta.Nombre,Centro=x.Centro.Nombre}).ToListAsync(ct));
    }
    [HttpGet("api/operaciones/autorizaciones"),Authorize(Policy="Operaciones.Aprobar")]
    public async Task<IActionResult> Autorizaciones([FromQuery]string? estado,[FromQuery]Guid? centroId,[FromQuery]string? tipo,[FromQuery]string? solicitante,[FromQuery]string? vehiculo,[FromQuery]DateTimeOffset? desde,[FromQuery]DateTimeOffset? hasta,[FromQuery]int pagina=1,CancellationToken ct=default,[FromQuery]Guid? grupoId=null)
    {
        var a=User.AlcanceCentros();
        var q=db.SolicitudesOperacion.AsNoTracking().Where(x=>x.Activo&&(a.VerTodos||a.CentroIds.Contains(x.CentroId)));
        if(grupoId.HasValue)q=q.Where(x=>x.GrupoOperacionId==grupoId);
        if(estado!="TODOS"){if(!Enum.TryParse<EstadoSolicitudOperacion>(estado??"PENDIENTE_APROBACION",out var state))throw new ValidacionException("Estado inválido.");q=q.Where(x=>x.Estado==state);}
        if(centroId.HasValue)q=q.Where(x=>x.CentroId==centroId);
        if(!string.IsNullOrWhiteSpace(tipo))q=q.Where(x=>x.Tipo==tipo);
        if(!string.IsNullOrWhiteSpace(solicitante))q=q.Where(x=>x.Solicitante.Contains(solicitante.Trim()));
        if(desde.HasValue)q=q.Where(x=>x.FechaCreacion>=desde);
        if(hasta.HasValue){var end=hasta.Value.AddDays(1);q=q.Where(x=>x.FechaCreacion<end);}
        if(!string.IsNullOrWhiteSpace(vehiculo)){var term=vehiculo.Trim();q=q.Where(x=>db.PosicionesVehiculo.Any(p=>(p.Id==x.PosicionDestinoId||p.Id==x.PosicionOrigenId)&&(p.EjeVehiculo.Vehiculo.Placa.Contains(term)||p.EjeVehiculo.Vehiculo.NumeroInterno.Contains(term))));}
        var total=await q.CountAsync(ct);
        var items=await q.OrderByDescending(x=>x.FechaCreacion).ThenBy(x=>x.Id).Skip((Math.Max(1,pagina)-1)*30).Take(grupoId.HasValue?100:30).Select(x=>new{
            LlantaActual=db.Llantas.Where(t=>t.Id==x.LlantaDesplazadaId).Select(t=>t.Codigo).FirstOrDefault(),
            x.Id,x.Tipo,x.GrupoOperacionId,Estado=x.Estado.ToString(),Fecha=x.FechaCreacion,x.CentroId,Centro=x.Centro.Nombre,x.Solicitante,x.LlantaId,Llanta=x.Llanta.Codigo,x.Llanta.Serial,
            Vehiculo=db.PosicionesVehiculo.Where(p=>p.Id==(x.PosicionDestinoId??x.PosicionOrigenId)).Select(p=>p.EjeVehiculo.Vehiculo.NumeroInterno+" / "+p.EjeVehiculo.Vehiculo.Placa).FirstOrDefault(),
            PosicionOrigen=db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionOrigenId).Select(p=>p.Codigo).FirstOrDefault(),
            PosicionDestino=db.PosicionesVehiculo.Where(p=>p.Id==x.PosicionDestinoId).Select(p=>p.Codigo).FirstOrDefault(),
            x.KilometrajeVehiculo,x.Motivo,x.Observaciones,x.ActividadProgramadaId,x.Aprobador,x.FechaDecision,x.MotivoRechazo}).ToListAsync(ct);
        return Ok(new{items,total,pagina=Math.Max(1,pagina),tamano=30});
    }
    private bool PuedePlanificarMontaje()=>User.HasClaim("permiso","operaciones.montar")||User.HasClaim("permiso","programacion.administrar");
    private string Usuario()=>User.Username();
    private async Task Ejecutar(SolicitudOperacion x,Application.Common.AlcanceCentros a,CancellationToken ct)
    {
        if(x.Tipo.Equals("Montaje",StringComparison.OrdinalIgnoreCase))
        {
            if(!x.PosicionDestinoId.HasValue||x.PosicionOrigenId.HasValue||x.CentroDestinoId.HasValue||x.LlantaDesplazadaId.HasValue)throw new ValidacionException("Solicitud de montaje inválida; requiere una posición libre.");
            await service.EjecutarReemplazosAsync([x],x.KilometrajeVehiculo??throw new ValidacionException("Ingresa un kilometraje válido."),Usuario(),a,ct);
            return;
        }
        if(x.CentroDestinoId.HasValue)
        {
            if(x.PosicionOrigenId.HasValue)
                await service.MoverAsync(new(){LlantaId=x.LlantaId,PosicionOrigenId=x.PosicionOrigenId,TipoDestino="Inventario",Motivo=x.Motivo,KilometrajeVehiculo=x.KilometrajeVehiculo,Observaciones=x.Observaciones},Usuario(),a,ct);
            await ciclo.TrasladarCentroAsync(x.LlantaId,new(x.CentroDestinoId.Value,x.Motivo,x.Observaciones),Usuario(),a,ct);
            x.MovimientoEjecutadoId=await db.Movimientos.Where(m=>m.Detalles.Any(d=>d.LlantaId==x.LlantaId&&d.CentroDestinoId==x.CentroDestinoId)).OrderByDescending(m=>m.FechaCreacion).Select(m=>(Guid?)m.Id).FirstOrDefaultAsync(ct);
        }
        else
        {
            var rotation=x.Tipo.Contains("rot",StringComparison.OrdinalIgnoreCase);
            var optionRepair=x.TipoDestino.Equals("Reparacion",StringComparison.OrdinalIgnoreCase);
            var move=await service.MoverAsync(new(){LlantaId=x.LlantaId,PosicionOrigenId=x.PosicionOrigenId,PosicionDestinoId=x.PosicionDestinoId,TipoDestino=optionRepair?"Inventario":x.TipoDestino,LlantaDesplazadaId=x.LlantaDesplazadaId,PosicionDestinoDesplazadaId=rotation?x.PosicionOrigenId:x.PosicionDestinoDesplazadaId,DestinoDesplazada=rotation?"Posicion":x.DestinoDesplazada,Motivo=x.Motivo,KilometrajeVehiculo=x.KilometrajeVehiculo,Observaciones=x.Observaciones},Usuario(),a,ct);
            x.MovimientoEjecutadoId=move.Id;
            if(optionRepair&&!await db.OrdenesServicioLlanta.AnyAsync(o=>o.LlantaId==x.LlantaId&&o.Tipo==TipoServicioLlanta.Reparacion&&o.Estado!="CERRADA"&&o.Estado!="RECHAZADA",ct))
                db.OrdenesServicioLlanta.Add(new OrdenServicioLlanta{Tipo=TipoServicioLlanta.Reparacion,Estado="OPCIONADA",LlantaId=x.LlantaId,CentroOrigenId=x.CentroId,Motivo=x.Motivo,Observaciones=x.Observaciones,Elegible=true,Solicitante=Usuario(),UsuarioOpciona=Usuario(),OrigenTipo="MOVIMIENTO",OrigenEntidadId=move.Id,FechaOpcionada=DateTimeOffset.UtcNow,UsuarioCreacion=Usuario()});
        }
        x.Estado=EstadoSolicitudOperacion.EJECUTADO;x.UsuarioModificacion=Usuario();x.FechaModificacion=DateTimeOffset.UtcNow;await db.SaveChangesAsync(ct);
    }
    private async Task<SolicitudOperacionDto> ObtenerSolicitud(Guid id,AlcanceCentros alcance,CancellationToken ct)
    {
        var item=await db.SolicitudesOperacion.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id,ct);
        if(item is null||!item.Activo)throw new SolicitudNoEncontradaException();
        if(!alcance.Autoriza(item.CentroId)&&!(item.CentroDestinoId.HasValue&&alcance.Autoriza(item.CentroDestinoId.Value)))throw new UnauthorizedAccessException();
        return await MapSolicitudAsync(item,ct);
    }
    private async Task<SolicitudOperacionDto> MapSolicitudAsync(SolicitudOperacion x,CancellationToken ct)
    {
        var centro=await db.Centros.AsNoTracking().Where(c=>c.Id==x.CentroId).Select(c=>c.Nombre).SingleOrDefaultAsync(ct)
            ??throw new ConflictoException("El centro de la solicitud ya no existe.");
        // Historical label only, after authorizing the request; never used to validate availability.
        var codigo=await db.Llantas.IgnoreQueryFilters().AsNoTracking().Where(t=>t.Id==x.LlantaId).Select(t=>t.Codigo).SingleOrDefaultAsync(ct)
            ??throw new ConflictoException("La llanta de la solicitud ya no existe.");
        return new(x.Id,x.Tipo,x.Estado.ToString(),x.CentroId,centro,x.LlantaId,codigo,x.PosicionOrigenId,x.PosicionDestinoId,x.TipoDestino,x.CentroDestinoId,x.Motivo,x.Observaciones,x.Solicitante,x.Aprobador,x.MotivoRechazo,x.FechaCreacion,x.FechaRecepcionDestino,Convert.ToBase64String(x.RowVersion));
    }
}
