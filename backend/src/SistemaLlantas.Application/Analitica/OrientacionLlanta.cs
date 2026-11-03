namespace SistemaLlantas.Application.Analitica;

public sealed record GiroOrientacion(Guid Id, DateTimeOffset? Fecha, decimal? Kilometraje, Guid? TecnicoId, bool EvidenciaCompleta = true);
public sealed record HombrosNormalizados(decimal? Exterior, decimal? Centro, decimal? Interior, bool Invertida, IReadOnlyList<Guid> GirosAplicados);

public static class OrientacionLlanta
{
    public static IReadOnlyList<MedicionAnalitica> Normalizar(IReadOnlyList<MedicionAnalitica> mediciones, IReadOnlyList<GiroOrientacion> giros, bool historialCompleto = true)
    {
        // Reference orientation is relative to the known record, never an inference of unknown earlier turns.
        var verificable = historialCompleto && giros.Select(g => g.Id).Distinct().Count() == giros.Count
            && giros.All(g => g.Id != Guid.Empty && g.EvidenciaCompleta && g.Fecha.HasValue && g.Fecha != default(DateTimeOffset)
                && g.Kilometraje >= 0 && g.TecnicoId.HasValue && g.TecnicoId != Guid.Empty);
        return mediciones.Select(m =>
        {
            var l = m.Lectura;var hasta = l.FechaHasta ?? l.Fecha;
            if (!verificable || l.Fecha == default || hasta < l.Fecha || giros.Any(g => g.Fecha >= l.Fecha && g.Fecha <= hasta))
                return m with { Normalizada = null, Orientacion = "No verificable", Calidad = m.Calidad + " · Orientación no verificable: falta evidencia o un giro coincide con el intervalo de la inspección." };
            var anteriores = giros.Where(g => g.Fecha < l.Fecha).OrderBy(g => g.Fecha).ThenBy(g => g.Id).ToArray();
            var invertida = anteriores.Length % 2 == 1;
            return m with { Normalizada = new(invertida ? l.Interior : l.Exterior, l.Centro, invertida ? l.Exterior : l.Interior, invertida, anteriores.Select(g => g.Id).ToArray()),
                Orientacion = invertida ? "Invertida respecto a la referencia" : "Referencia automática (sin inferir giros desconocidos)" };
        }).ToArray();
    }
}
