using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Application.Common;

public static class VehiculoVisibilidad
{
    public static IQueryable<Vehiculo> VisiblePara(this IQueryable<Vehiculo> query, AlcanceCentros alcance)
        => query.Where(v => v.SGrupoOperativo == "T1" || alcance.VerTodos || alcance.CentroIds.Contains(v.CentroId));
}
