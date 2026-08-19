using System.Text;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Infrastructure.Persistence;

namespace SistemaLlantas.Api.Controllers;

[ApiController,Route("api/reportes"),Authorize(Policy="Reportes.Exportar")]
public sealed class ReportesController(LlantasDbContext db):ControllerBase
{
 [HttpGet("{tipo}")]public async Task<IActionResult> Exportar(string tipo,[FromQuery]string formato="xlsx",[FromQuery]Guid? centroId=null,[FromQuery]DateTimeOffset? desde=null,[FromQuery]DateTimeOffset? hasta=null,CancellationToken ct=default){var a=User.AlcanceCentros();if(centroId.HasValue&&!a.Autoriza(centroId.Value))throw new UnauthorizedAccessException("Centro no autorizado.");var rows=tipo.ToLowerInvariant() switch{"vehiculos"=>await Vehicles(),"movimientos"=>await Movements(),"servicios"=>await Services(),_=>throw new ValidacionException("Reporte no soportado.")};if(formato.Equals("csv",StringComparison.OrdinalIgnoreCase)){var csv=new StringBuilder();csv.AppendLine(string.Join(',',rows.Headers.Select(Escape)));foreach(var row in rows.Rows)csv.AppendLine(string.Join(',',row.Select(Escape)));return File(Encoding.UTF8.GetBytes(csv.ToString()),"text/csv",$"{tipo}.csv");}using var wb=new XLWorkbook();var ws=wb.AddWorksheet("Reporte");for(var c=0;c<rows.Headers.Length;c++)ws.Cell(1,c+1).Value=rows.Headers[c];for(var r=0;r<rows.Rows.Count;r++)for(var c=0;c<rows.Rows[r].Length;c++)ws.Cell(r+2,c+1).Value=rows.Rows[r][c];ws.Row(1).Style.Font.Bold=true;ws.SheetView.FreezeRows(1);ws.Columns().AdjustToContents();using var stream=new MemoryStream();wb.SaveAs(stream);return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",$"{tipo}.xlsx");
 async Task<Data> Vehicles(){var q=db.Vehiculos.AsNoTracking().Where(x=>a.VerTodos||a.CentroIds.Contains(x.CentroId));if(centroId.HasValue)q=q.Where(x=>x.CentroId==centroId);return new(["Interno","Placa","Tipo","Centro","Estado","Kilometraje"],await q.OrderBy(x=>x.NumeroInterno).Select(x=>new[]{x.NumeroInterno,x.Placa,x.Tipo,x.Centro.Nombre,x.Estado,x.Kilometraje.ToString()!}).ToListAsync(ct));}
 async Task<Data> Movements(){var q=db.Movimientos.AsNoTracking().Where(x=>a.VerTodos||a.CentroIds.Contains(x.CentroId));if(centroId.HasValue)q=q.Where(x=>x.CentroId==centroId);if(desde.HasValue)q=q.Where(x=>x.FechaCreacion>=desde);if(hasta.HasValue)q=q.Where(x=>x.FechaCreacion<=hasta);return new(["Número","Tipo","Fecha","Centro","Motivo","Usuario"],await q.OrderByDescending(x=>x.FechaCreacion).Select(x=>new[]{x.Numero,x.Tipo,x.FechaCreacion.ToString("O"),x.Centro.Nombre,x.Motivo,x.Usuario}).ToListAsync(ct));}
 async Task<Data> Services(){var q=db.OrdenesServicioLlanta.AsNoTracking().Where(x=>a.VerTodos||a.CentroIds.Contains(x.CentroOrigenId));if(centroId.HasValue)q=q.Where(x=>x.CentroOrigenId==centroId);if(desde.HasValue)q=q.Where(x=>x.FechaCreacion>=desde);if(hasta.HasValue)q=q.Where(x=>x.FechaCreacion<=hasta);return new(["Tipo","Estado","Llanta","Centro","Proveedor","Costo","Fecha"],await q.OrderByDescending(x=>x.FechaCreacion).Select(x=>new[]{x.Tipo.ToString(),x.Estado,x.Llanta.Codigo,x.CentroOrigen.Nombre,x.Proveedor!=null?x.Proveedor.Nombre:"",x.Costo.ToString()!,x.FechaCreacion.ToString("O")}).ToListAsync(ct));}}
 private static string Escape(string? x)=>$"\"{(x??string.Empty).Replace("\"","\"\"")}\"";private sealed record Data(string[] Headers,List<string[]> Rows);
}
