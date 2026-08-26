using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Application.Llantas;
using SistemaLlantas.Api.Security;
using ClosedXML.Excel;
using System.Text;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/llantas"), Authorize(Policy = "Llantas.Consultar")]
public sealed class LlantasController(ILlantaService service,ICicloVidaLlantaService cicloVida) : ControllerBase
{
    [HttpGet]
    public Task<Pagina<LlantaResumenDto>> Consultar([FromQuery] ConsultaPaginada consulta, CancellationToken ct) => service.ConsultarAsync(consulta, User.AlcanceCentros(), ct);
    [HttpGet("metricas")] public Task<LlantaMetricasDto> Metricas([FromQuery]ConsultaPaginada consulta,CancellationToken ct)=>service.MetricasAsync(consulta,User.AlcanceCentros(),ct);

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LlantaResumenDto>> Obtener(Guid id, CancellationToken ct) => await service.ObtenerAsync(id, User.AlcanceCentros(), ct) is { } item ? Ok(item) : NotFound();

    [HttpGet("{id:guid}/historial")]
    public async Task<ActionResult<LlantaDetalleDto>> Historial(Guid id,CancellationToken ct)=>await cicloVida.ObtenerDetalleAsync(id,User.AlcanceCentros(),ct) is { } item?Ok(item):NotFound();

    [HttpPost("{id:guid}/traslados"),Authorize(Policy="Llantas.Administrar")]
    public async Task<IActionResult> Trasladar(Guid id,TrasladarLlantaDto dto,CancellationToken ct){await cicloVida.TrasladarCentroAsync(id,dto,Usuario(),User.AlcanceCentros(),ct);return NoContent();}

    [HttpPost("{id:guid}/conciliar-montaje"),Authorize(Policy="Llantas.Administrar")]
    public async Task<IActionResult> ConciliarMontaje(Guid id,CancellationToken ct){await cicloVida.ConciliarMontajeAsync(id,Usuario(),User.AlcanceCentros(),ct);return NoContent();}

    [HttpGet("exportar")]
    public async Task<IActionResult> Exportar([FromQuery] ConsultaPaginada consulta,[FromQuery] string formato="xlsx",CancellationToken ct=default)
    {
        var items=await service.ExportarAsync(consulta,User.AlcanceCentros(),ct);
        if(string.Equals(formato,"csv",StringComparison.OrdinalIgnoreCase))
        {
            static string C(string? value)=>$"\"{(value??string.Empty).Replace("\"","\"\"")}\"";
            var csv=new StringBuilder("Codigo,Serial,Marca,Referencia,Dimension,Tipo,Estado,Centro,Ubicacion,Vehiculo,Posicion,Profundidad,KilometrajeAcumulado,Reencauches,UltimaInspeccion\r\n");
            foreach(var x in items)csv.AppendLine(string.Join(',',C(x.Codigo),C(x.Serial),C(x.Marca),C(x.Referencia),C(x.Dimension),C(x.Tipo),C(x.Estado),C(x.Centro),C(x.UbicacionActual),C(x.VehiculoActual),C(x.PosicionActual),x.ProfundidadInicial,x.KilometrajeAcumulado,x.NumeroReencauches,C(x.UltimaInspeccion?.ToString("O"))));
            return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray(),"text/csv","llantas.csv");
        }
        using var book=new XLWorkbook();var sheet=book.Worksheets.Add("Llantas");var headers=new[]{"Código","Serial","Marca","Referencia","Dimensión","Tipo","Estado","Centro","Ubicación","Vehículo","Posición","Profundidad","Kilometraje acumulado","Reencauches","Última inspección"};
        for(var c=0;c<headers.Length;c++)sheet.Cell(1,c+1).Value=headers[c];var row=2;foreach(var x in items){var values=new object?[]{x.Codigo,x.Serial,x.Marca,x.Referencia,x.Dimension,x.Tipo,x.Estado,x.Centro,x.UbicacionActual,x.VehiculoActual,x.PosicionActual,x.ProfundidadInicial,x.KilometrajeAcumulado,x.NumeroReencauches,x.UltimaInspeccion};for(var c=0;c<values.Length;c++)sheet.Cell(row,c+1).Value=XLCellValue.FromObject(values[c]);row++;}sheet.RangeUsed()?.CreateTable();sheet.Columns().AdjustToContents();using var stream=new MemoryStream();book.SaveAs(stream);return File(stream.ToArray(),"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet","llantas.xlsx");
    }

    [HttpPost, Authorize(Policy = "Llantas.Administrar")]
    public async Task<ActionResult<LlantaResumenDto>> Crear(GuardarLlantaDto dto, CancellationToken ct)
    {
        var item = await service.CrearAsync(dto, Usuario(), User.AlcanceCentros(), ct); return CreatedAtAction(nameof(Obtener), new { id = item.Id }, item);
    }

    [HttpPut("{id:guid}"), Authorize(Policy = "Llantas.Administrar")]
    public async Task<ActionResult<LlantaResumenDto>> Actualizar(Guid id, GuardarLlantaDto dto, CancellationToken ct) => await service.ActualizarAsync(id, dto, Usuario(), User.AlcanceCentros(), ct) is { } item ? Ok(item) : NotFound();

    [HttpPatch("{id:guid}/estado"), Authorize(Policy = "Llantas.Administrar")]
    public async Task<IActionResult> Estado(Guid id, [FromBody] CambiarEstadoRequest request, CancellationToken ct) => await service.CambiarEstadoAsync(id, request.Activo, Usuario(), User.AlcanceCentros(), ct) ? NoContent() : NotFound();

    private string Usuario() => User.Username();
    public sealed record CambiarEstadoRequest(bool Activo);
}
