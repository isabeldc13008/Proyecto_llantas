using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Analitica;

public sealed record GrupoDesgaste(string Id, string Nombre, int Total, int MuestraProfundidad,
    decimal? ProfundidadMedia, int MuestraKm, decimal? KmMedio);
public sealed record LlantaDesgaste(Guid Id, string Codigo, decimal? Profundidad, decimal? Km);
public sealed record DesgasteFlota(DateTimeOffset GeneradoEn, string Procedencia, GrupoDesgaste Total,
    Pagina<GrupoDesgaste> Grupos, Pagina<LlantaDesgaste> Llantas);
public sealed record IntervencionDesgaste(Guid Id, Guid MovimientoId, string Numero, string Tipo,
    DateTimeOffset Fecha, string Motivo, string Usuario, decimal? KilometrajeGiro, string? Origen, string? Destino);
public sealed record TasaDesgaste(Guid Desde, Guid Hasta, string Zona, decimal? Km, decimal? MmPorMilKm, string? Motivo);

public static class TasasDesgaste
{
    // Adjacent evidence only: never bridge an incomplete reading or a different mount/cycle.
    public static IReadOnlyList<TasaDesgaste> Calcular(IReadOnlyList<MedicionAnalitica> mediciones)
    {
        var ordered = mediciones.OrderBy(x => x.Lectura.Fecha).ThenBy(x => x.Lectura.Id).ToArray();
        var result = new List<TasaDesgaste>();
        for (var i = 1; i < ordered.Length; i++)
        {
            var a = ordered[i - 1]; var b = ordered[i];
            var km = b.KmDesdeMontaje - a.KmDesdeMontaje;
            string? reason = a.TramoId is null || a.TramoId != b.TramoId ? "Sin un mismo tramo comprobado; no se unen montajes distintos."
                : b.Lectura.Fecha <= (a.Lectura.FechaHasta ?? a.Lectura.Fecha) ? "Fechas coincidentes o intervalos de inspección solapados."
                : !km.HasValue || km <= 0 ? "Falta kilometraje comparable con incremento positivo."
                : a.Normalizada is null || b.Normalizada is null ? "Orientación histórica no verificable." : null;
            foreach (var zone in new[] { "Exterior", "Centro", "Interior" })
            {
                decimal? Value(MedicionAnalitica m) => zone switch {
                    "Exterior" => m.Normalizada?.Exterior, "Centro" => m.Normalizada?.Centro, _ => m.Normalizada?.Interior };
                var start = Value(a); var end = Value(b);
                var cause = reason ?? (!start.HasValue || !end.HasValue || start < 0 || end < 0 ? "Profundidad ausente o inválida en esta zona."
                    : end > start ? "Aumento de profundidad: comparabilidad pendiente de verificación." : null);
                result.Add(new(a.Lectura.Id, b.Lectura.Id, zone, reason is null ? km : null,
                    cause is null ? (start - end) * 1000m / km : null, cause));
            }
        }
        return result;
    }
}
