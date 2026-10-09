using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Disposicion;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Services;
public sealed partial class DisposicionService
{
 private static IOrderedQueryable<OrdenServicioLlanta> Ordenar(IQueryable<OrdenServicioLlanta> q,ConsultaDisposicion f)=>f.OrdenarPor switch{
 "llanta"=>f.Descendente?q.OrderByDescending(o=>o.Llanta.Codigo):q.OrderBy(o=>o.Llanta.Codigo),
 "centro"=>f.Descendente?q.OrderByDescending(o=>o.CentroOrigen.Nombre):q.OrderBy(o=>o.CentroOrigen.Nombre),
 "estado"=>f.Descendente?q.OrderByDescending(o=>o.Estado):q.OrderBy(o=>o.Estado),
 "evidencia"=>f.Descendente?q.OrderByDescending(o=>o.Evidencias.Count(e=>e.Activo)):q.OrderBy(o=>o.Evidencias.Count(e=>e.Activo)),
 null or "fecha"=>f.Descendente?q.OrderByDescending(o=>o.FechaModificacion??o.FechaCreacion):q.OrderBy(o=>o.FechaModificacion??o.FechaCreacion),
 _=>throw new ValidacionException("Columna de ordenamiento no válida.")};
 public async Task<Pagina<ValorFiltroDisposicion>> FiltrosAsync(string columna,string? buscar,int pagina,ConsultaDisposicion f,AlcanceCentros a,CancellationToken ct)
 {
  f=columna switch{"llanta"=>f with{LlantaIds=null,LlantaTexto=null},"centro"=>f with{CentroId=null,CentroIds=null,CentroTexto=null},"estado"=>f with{Estado=null,Estados=null,EstadoTexto=null},_=>throw new ValidacionException("Columna de filtro no válida.")};
  var q=Filtrar(Ordenes(a),f);var values=columna switch{
   "llanta"=>q.Select(o=>new {Valor=o.LlantaId.ToString(),Etiqueta=o.Llanta.Codigo+" · "+o.Llanta.Serial}),
   "centro"=>q.Select(o=>new {Valor=o.CentroOrigenId.ToString(),Etiqueta=o.CentroOrigen.Nombre}),
   _=>q.Select(o=>new {Valor=o.Estado,Etiqueta=o.Estado})};
  if(columna=="estado")buscar=buscar?.Replace(" ","_");values=values.Distinct();if(!string.IsNullOrWhiteSpace(buscar))values=values.Where(x=>x.Etiqueta.Contains(buscar));var total=await values.CountAsync(ct);pagina=Math.Max(1,pagina);var rows=await values.OrderBy(x=>x.Etiqueta).ThenBy(x=>x.Valor).Skip((pagina-1)*40).Take(40).ToListAsync(ct);
  return new(rows.Select(v=>new ValorFiltroDisposicion(v.Valor,columna=="estado"?Etiqueta(v.Valor):v.Etiqueta)).ToArray(),pagina,40,total);
 }
}


