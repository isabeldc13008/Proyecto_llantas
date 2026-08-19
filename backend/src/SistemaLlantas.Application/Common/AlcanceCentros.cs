namespace SistemaLlantas.Application.Common;

public sealed record AlcanceCentros(bool VerTodos, IReadOnlyCollection<Guid> CentroIds)
{
    public bool SinAcceso => !VerTodos && CentroIds.Count == 0;
    public bool Autoriza(Guid centroId) => VerTodos || CentroIds.Contains(centroId);
}
