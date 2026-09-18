namespace SistemaLlantas.Application.Analitica;

/// <summary>Descriptive statistics only. No inferred lifespan, imputation or prediction.</summary>
public static class EstadisticasAnalitica
{
    public static EstadisticaKm Calcular(IEnumerable<decimal?> valores)
    {
        var datos = valores.Where(x => x >= 0).Select(x => x!.Value).Order().ToArray();
        if (datos.Length == 0) return new(0, null, null, null, null, null);
        var media = datos.Average();
        var mitad = datos.Length / 2;
        var mediana = datos.Length % 2 == 0 ? (datos[mitad - 1] + datos[mitad]) / 2 : datos[mitad];
        // Population deviation of the observed cohort, not an uncertainty/confidence interval.
        var varianza = datos.Average(x => Math.Pow((double)(x - media), 2));
        return new(datos.Length, Math.Round(media, 2), Math.Round(mediana, 2), Math.Round((decimal)Math.Sqrt(varianza), 2), datos[0], datos[^1]);
    }

    public static decimal? RecorridoCompleto(int tramos, int validos, decimal suma)
        => tramos > 0 && tramos == validos && suma >= 0 ? suma : null;
}
