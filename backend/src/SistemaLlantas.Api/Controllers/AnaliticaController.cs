using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/analitica"), Authorize(Policy = "Analitica.Consultar")]
public sealed class AnaliticaController(IAnaliticaService service) : ControllerBase
{
    [HttpGet("mantenimiento")]
    public Task<ColaMantenimiento> Mantenimiento([FromQuery] FiltroMantenimiento filtro, CancellationToken ct) => service.MantenimientoAsync(filtro, User.AlcanceCentros(), ct);
    [HttpGet("mantenimiento/vehiculos")]
    public Task<Pagina<ReferenciaVehiculo>> Vehiculos([FromQuery] string? buscar, [FromQuery] Guid? centroId, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default) => service.VehiculosMantenimientoAsync(buscar, centroId, pagina, tamano, User.AlcanceCentros(), ct);
    [HttpGet("mantenimiento/llantas/{id:guid}")]
    public Task<DetalleMantenimiento> DetalleMantenimiento(Guid id, CancellationToken ct) => service.DetalleMantenimientoAsync(id, User.AlcanceCentros(), ct);
    [HttpGet("mantenimiento/llantas/{id:guid}/alertas")]
    public Task<Pagina<AlertaObservada>> AlertasMantenimiento(Guid id, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20, CancellationToken ct = default) => service.AlertasMantenimientoAsync(id, pagina, tamano, User.AlcanceCentros(), ct);
    [HttpGet("llantas")]
    public Task<Pagina<LlantaAnalitica>> Llantas([FromQuery] FiltroAnalitica filtro, [FromQuery] string indicador = "todas", CancellationToken ct = default) => service.LlantasAsync(filtro, indicador, User.AlcanceCentros(), ct);
    [HttpGet("llantas/{id:guid}/desgaste")]
    public Task<DesgasteAnalitica> Desgaste(Guid id, CancellationToken ct) => service.DesgasteAsync(id, User.AlcanceCentros(), ct);
    [HttpGet("opciones")]
    public Task<OpcionesAnalitica> Opciones(CancellationToken ct) => service.OpcionesAsync(User.AlcanceCentros(), ct);
    [HttpGet("resumen")]
    public Task<ResumenAnalitica> Resumen([FromQuery] FiltroAnalitica filtro, CancellationToken ct) => service.ResumenAsync(filtro, User.AlcanceCentros(), ct);
    [HttpGet("vida-util")]
    public Task<Pagina<GrupoAnalitica>> Vida([FromQuery] FiltroAnalitica filtro, [FromQuery] string agrupar = "marca", CancellationToken ct = default) => service.CompararAsync(filtro, agrupar, User.AlcanceCentros(), ct);
    [HttpGet("marcas-referencias")]
    public Task<Pagina<GrupoAnalitica>> Marcas([FromQuery] FiltroAnalitica filtro, [FromQuery] string agrupar = "marca-referencia", CancellationToken ct = default) => service.CompararAsync(filtro, agrupar, User.AlcanceCentros(), ct);
    [HttpGet("centros")]
    public Task<Pagina<GrupoAnalitica>> Centros([FromQuery] FiltroAnalitica filtro, CancellationToken ct) => service.CompararAsync(filtro, "centro", User.AlcanceCentros(), ct);
    [HttpGet("posiciones")]
    public Task<Pagina<PosicionAnalitica>> Posiciones([FromQuery] FiltroAnalitica filtro, CancellationToken ct) => service.PosicionesAsync(filtro, User.AlcanceCentros(), ct);
    [HttpGet("movimientos")]
    public Task<RankingMovimientos> Movimientos([FromQuery] FiltroAnalitica filtro, CancellationToken ct) => service.MovimientosAsync(filtro, User.AlcanceCentros(), ct);
}
