namespace SistemaLlantas.Application.Analitica;

public static class DesgasteAnaliticaCalculo
{
    public static IReadOnlyList<MedicionAnalitica> Vincular(IReadOnlyList<LecturaAnalitica> lecturas, IReadOnlyList<TramoAnalitica> tramos)
    {
        var resultados = lecturas.OrderBy(x => x.Fecha).ThenBy(x => x.Id).Select(l =>
        {
            decimal? minima = l.Exterior >= 0 && l.Centro >= 0 && l.Interior >= 0
                ? Math.Min(l.Exterior.Value, Math.Min(l.Centro.Value, l.Interior.Value)) : null;
            // No escoger arbitrariamente una asignación en límites compartidos o solapamientos.
            var candidatos = tramos.Where(t => t.PosicionId == l.PosicionId && t.Inicio <= l.Fecha
                && (t.Fin.HasValue ? l.Fecha <= t.Fin : t.Activo)).ToArray();
            var t = candidatos.Length == 1 ? candidatos[0] : null;
            var valido = t is not null && t.Montaje >= 0 && l.Odometro >= t.Montaje
                && (t.Activo ? !t.Fin.HasValue && t.OdometroActual >= l.Odometro : t.Fin >= t.Inicio && t.Desmontaje >= l.Odometro);
            return new MedicionAnalitica(l, minima, valido ? t!.Id : null,
                valido ? l.Odometro - t!.Montaje : null,
                !valido ? "Sin tramo inequívoco con odómetros consistentes" : minima is null ? "Medición incompleta o negativa" : "Lectura completa y tramo vinculado");
        }).ToArray();
        // Un retroceso o dos odómetros distintos en la misma fecha invalidan la serie completa del tramo.
        var inconsistentes = resultados.Where(x => x.TramoId.HasValue).GroupBy(x => x.TramoId)
            .Where(g => g.Zip(g.Skip(1)).Any(p => p.Second.Lectura.Odometro < p.First.Lectura.Odometro
                || p.Second.Lectura.Fecha == p.First.Lectura.Fecha && p.Second.Lectura.Odometro != p.First.Lectura.Odometro))
            .Select(g => g.Key).ToHashSet();
        return resultados.Select(x => inconsistentes.Contains(x.TramoId)
            ? x with { TramoId = null, KmDesdeMontaje = null, Calidad = "Odómetros discontinuos dentro del tramo" } : x).ToArray();
    }
}
