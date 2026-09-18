using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Api.Controllers;

[ApiController, Route("api/analitica"), Authorize(Policy = "Analitica.Consultar")]
public sealed class AnaliticaController(IAnaliticaService service) : ControllerBase
{
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
