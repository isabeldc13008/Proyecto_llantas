using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Api.Security;
using SistemaLlantas.Application.Operaciones;
namespace SistemaLlantas.Api.Controllers;

public sealed partial class OperacionesController
{
    [HttpPost("api/operaciones/giro-rin"), Authorize(Policy = "Operaciones.Ejecutar")]
    public Task<MovimientoDto> GiroRin(GiroRinDto dto, CancellationToken ct)
    {
        if (User.IsInRole("TECNICO")) dto = dto with { TecnicoId = TecnicoGiroAutenticado() };
        return service.GirarEnRinAsync(dto, Usuario(), User.AlcanceCentros(), ct);
    }

    private Guid TecnicoGiroAutenticado() => Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) && id != Guid.Empty
        ? id : throw new UnauthorizedAccessException("No fue posible identificar al técnico autenticado.");

    [HttpGet("api/operaciones/giro-rin/tecnicos"), Authorize(Policy = "Operaciones.Ejecutar")]
    public async Task<IReadOnlyList<TecnicoGiroDto>> TecnicosGiro(Guid vehiculoId, CancellationToken ct)
    {
        var scope = User.AlcanceCentros();
        Guid? currentTechnician = User.IsInRole("TECNICO") ? TecnicoGiroAutenticado() : null;
        var center = await db.Vehiculos.Where(v => v.Id == vehiculoId && v.Activo && (scope.VerTodos || scope.CentroIds.Contains(v.CentroId))).Select(v => (Guid?)v.CentroId).SingleOrDefaultAsync(ct)
            ?? throw new UnauthorizedAccessException("Vehículo fuera de los centros autorizados.");
        return await db.UsuariosSistema.AsNoTracking().Where(x => (!currentTechnician.HasValue || x.Id == currentTechnician.Value) && x.Activo && x.Rol.Activo && x.Rol.Codigo == "TECNICO" && x.Centros.Any(c => c.Activo && c.CentroId == center)).OrderBy(x => x.Nombre).Select(x => new TecnicoGiroDto(x.Id, x.Nombre)).ToListAsync(ct);
    }
}
