using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Dashboard;

namespace SistemaLlantas.Api.Controllers;
[ApiController,Route("api/dashboard"),Authorize(Policy="Dashboard.Consultar")]
public sealed class DashboardController(IDashboardService service):ControllerBase
{
 [HttpGet("resumen")]public Task<DashboardResumenDto> Resumen([FromQuery]Guid? centroId,CancellationToken ct)=>service.ObtenerAsync(centroId,User.AlcanceCentros(),ct);
}
