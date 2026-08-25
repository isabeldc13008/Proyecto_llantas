using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Dashboard;
using SistemaLlantas.Domain.Entities;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Infrastructure.Services;

public sealed class DashboardService(LlantasDbContext db):IDashboardService
{
 public async Task<DashboardResumenDto> ObtenerAsync(Guid? centroId,AlcanceCentros alcance,CancellationToken ct)
 {
  if(centroId.HasValue&&(!alcance.Autoriza(centroId.Value)||!await db.Centros.AnyAsync(x=>x.Id==centroId&&x.Activo,ct)))throw new UnauthorizedAccessException("El centro solicitado no pertenece a su alcance.");
  var allowed=await db.Centros.AsNoTracking().Where(x=>x.Activo&&(alcance.VerTodos||alcance.CentroIds.Contains(x.Id))).Select(x=>x.Id).ToListAsync(ct);
  if(centroId.HasValue)allowed=allowed.Where(x=>x==centroId.Value).ToList();
  var now=DateTimeOffset.UtcNow;var start=now.Date;var end=start.AddDays(1);
  var tires=db.Llantas.AsNoTracking().Where(x=>allowed.Contains(x.CentroId));
  var vehicles=db.Vehiculos.AsNoTracking().Where(x=>x.Activo&&allowed.Contains(x.CentroId));
  var activities=db.ActividadesProgramadas.AsNoTracking().Where(x=>x.Activo&&allowed.Contains(x.CentroId));
  var openAlerts=db.AlertasInspeccion.AsNoTracking().Where(x=>x.Activo&&allowed.Contains(x.CentroId)&&(x.Estado==EstadoAlerta.ABIERTA||x.Estado==EstadoAlerta.EN_PROCESO));
  var mounted=await db.AsignacionesLlantaPosicion.AsNoTracking().CountAsync(x=>x.Activo&&x.EsActiva&&allowed.Contains(x.Llanta.CentroId),ct);
  var available=await tires.CountAsync(x=>x.EstadoLlanta.PermiteMontaje&&!db.AsignacionesLlantaPosicion.Any(a=>a.LlantaId==x.Id&&a.Activo&&a.EsActiva),ct);
  var repair=await tires.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("REPAR"),ct);var retread=await tires.CountAsync(x=>x.EstadoLlanta.Codigo.Contains("REENCAUCH"),ct);var disposal=await tires.CountAsync(x=>x.EstadoLlanta.EsDisposicionFinal||x.EstadoLlanta.Codigo.Contains("DISPOS"),ct);
  var vehicleState=vehicles.Select(v=>new{v.Id,v.CentroId,Required=v.Ejes.SelectMany(e=>e.Posiciones).Count(p=>p.Activo),Covered=v.Ejes.SelectMany(e=>e.Posiciones).Count(p=>p.Activo&&p.LlantaActualId!=null)});
  var controlled=await vehicleState.CountAsync(ct);var incomplete=await vehicleState.CountAsync(x=>x.Required>0&&x.Covered<x.Required,ct);var complete=await vehicleState.CountAsync(x=>x.Required>0&&x.Covered==x.Required,ct);
  var overdue=activities.Where(x=>(x.Estado==EstadoActividad.Pendiente||x.Estado==EstadoActividad.Vencida)&&x.FechaProgramada<now);
  var inspectionsOverdue=await overdue.CountAsync(x=>x.TipoActividad.Contains("Inspe"),ct);var pending=await activities.CountAsync(x=>x.Estado==EstadoActividad.Pendiente||x.Estado==EstadoActividad.EnEjecucion||x.Estado==EstadoActividad.Vencida,ct);
  var alertCount=await openAlerts.CountAsync(ct);var attentionCount=alertCount+incomplete+await overdue.CountAsync(ct);
  var alertVehicleIds=openAlerts.Select(x=>x.VehiculoId).Distinct();var vehiclesAlert=await vehicleState.CountAsync(x=>alertVehicleIds.Contains(x.Id),ct);
  var attention=await openAlerts.OrderByDescending(x=>x.Tipo.Contains("CRIT")).ThenBy(x=>x.FechaCreacion).Take(6).Select(x=>new DashboardAtencionDto(x.Tipo.Contains("CRIT")||x.Tipo.Contains("PROFUNDIDAD")?"CRITICA":"ALTA","ALERTA",x.InspeccionDetalle.Llanta!=null?x.InspeccionDetalle.Llanta.Codigo:null,x.Inspeccion.Vehiculo.Placa,x.Inspeccion.Centro.Nombre,x.Descripcion,x.FechaCreacion,"/alertas")).ToListAsync(ct);
  if(attention.Count<8)attention.AddRange(await vehicles.Where(v=>v.Ejes.SelectMany(e=>e.Posiciones).Any()&&v.Ejes.SelectMany(e=>e.Posiciones).Any(p=>p.Activo&&p.LlantaActualId==null)).OrderBy(v=>v.Placa).Take(8-attention.Count).Select(v=>new DashboardAtencionDto("ALTA","VEHICULO",null,v.Placa,v.Centro.Nombre,"Vehículo con una o más posiciones sin llanta",v.FechaModificacion??v.FechaCreacion,"/vehiculos?estado=incompleto")).ToListAsync(ct));
  if(attention.Count<8)attention.AddRange(await overdue.OrderBy(x=>x.FechaProgramada).Take(8-attention.Count).Select(x=>new DashboardAtencionDto("MEDIA","PROGRAMACION",x.LlantaId.HasValue?db.Llantas.Where(t=>t.Id==x.LlantaId).Select(t=>t.Codigo).FirstOrDefault():null,x.Vehiculo!=null?x.Vehiculo.Placa:null,x.Centro.Nombre,x.TipoActividad+" vencida",x.FechaProgramada,"/programacion?estado=vencida")).ToListAsync(ct));
  var today=await activities.Where(x=>x.FechaProgramada>=start&&x.FechaProgramada<end||((x.Estado==EstadoActividad.Pendiente||x.Estado==EstadoActividad.Vencida)&&x.FechaProgramada<start)).OrderBy(x=>x.FechaProgramada).Take(8).Select(x=>new DashboardHoyDto(x.Id,x.TipoActividad,x.FechaProgramada,x.Centro.Nombre,x.Vehiculo!=null?x.Vehiculo.NumeroInterno+" · "+x.Vehiculo.Placa:null,x.Estado.ToString(),"/programacion")).ToListAsync(ct);
  var centerRows=await db.Centros.AsNoTracking().Where(c=>allowed.Contains(c.Id)).OrderBy(c=>c.Nombre).Select(c=>new{c.Id,c.Nombre,Llantas=db.Llantas.Count(t=>t.CentroId==c.Id),Vehiculos=db.Vehiculos.Count(v=>v.Activo&&v.CentroId==c.Id),Criticas=db.AlertasInspeccion.Count(a=>a.Activo&&a.CentroId==c.Id&&(a.Estado==EstadoAlerta.ABIERTA||a.Estado==EstadoAlerta.EN_PROCESO)&&(a.Tipo.Contains("CRIT")||a.Tipo.Contains("PROFUNDIDAD"))),Vencidas=db.ActividadesProgramadas.Count(a=>a.Activo&&a.CentroId==c.Id&&a.TipoActividad.Contains("Inspe")&&a.FechaProgramada<now&&(a.Estado==EstadoActividad.Pendiente||a.Estado==EstadoActividad.Vencida)),Pendientes=db.ActividadesProgramadas.Count(a=>a.Activo&&a.CentroId==c.Id&&(a.Estado==EstadoActividad.Pendiente||a.Estado==EstadoActividad.EnEjecucion||a.Estado==EstadoActividad.Vencida))}).ToListAsync(ct);
  var centers=centerRows.Select(x=>new DashboardCentroDto(x.Id,x.Nombre,x.Llantas,x.Vehiculos,x.Criticas,x.Vencidas,x.Pendientes,x.Criticas>0?"Crítico":x.Vencidas>0||x.Pendientes>0?"Atención":"Estable")).ToList();
  var total=await tires.CountAsync(ct);var other=Math.Max(0,total-mounted-available-repair-retread);
  return new(new(total,mounted,available,attentionCount,inspectionsOverdue,incomplete,repair,retread,disposal,pending),attention.OrderBy(x=>x.Prioridad=="CRITICA"?0:x.Prioridad=="ALTA"?1:2).Take(8).ToList(),today,new(controlled,complete,incomplete,vehiclesAlert,controlled>0?Math.Round(complete*100m/controlled,1):null),new(mounted,available,repair,retread,other),centers);
 }
}
