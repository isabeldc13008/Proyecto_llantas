using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Application.Analitica;
using SistemaLlantas.Application.Common;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Services;

public sealed partial class AnaliticaService
{
    public async Task<Pagina<LlantaAnalitica>> LlantasAsync(FiltroAnalitica f, string indicador, AlcanceCentros a, CancellationToken ct)
    {
        if (indicador is not ("todas" or "montadas" or "disponibles" or "reparacion" or "reencauche" or "finalizadas" or "alertas" or "sin-km" or "incompletos" or "medidas"))
            throw new ValidacionException("Indicador no admitido.");
        var rows = (await DatosAsync(f, a, ct)).Where(x => indicador switch
        {
            "montadas" => x.Tire.Montada, "disponibles" => x.Tire.Disponible,
            "reparacion" => x.Tire.EstadoCodigo is "EN_REPARACION" or "REP",
            "reencauche" => x.Tire.EstadoCodigo is "EN_REENCAUCHE" or "REE",
            "finalizadas" => x.Tire.Finalizada, "alertas" => x.Alertas > 0,
            "sin-km" => x.Km is null, "incompletos" => x.Tramos != x.Validos,
            "medidas" => x.Tire.Profundidad.HasValue, _ => true
        }).OrderByDescending(x => x.Alertas).ThenBy(x => x.Tire.Codigo).ThenBy(x => x.Tire.Id).ToArray();
        return new(rows.Skip((f.Pagina - 1) * f.Tamano).Take(f.Tamano)
            .Select(x => new LlantaAnalitica(x.Tire.Id, x.Tire.Codigo, x.Tire.Serial, x.Tire.Centro, x.Tire.Estado, x.Alertas, x.Tire.Profundidad, x.Km)).ToArray(), f.Pagina, f.Tamano, rows.Length);
    }

    public async Task<DesgasteAnalitica> DesgasteAsync(Guid id, AlcanceCentros a, CancellationToken ct)
    {
        var tire = await Llantas(new(), a).Where(x => x.Id == id).Select(x => new { x.Id, x.Codigo, x.CentroId }).SingleOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException("Llanta no encontrada en el alcance autorizado.");
        const int limite = 1000;
        var lecturas = await db.InspeccionesDetalle.AsNoTracking().Where(d => d.Activo && d.LlantaId == id
            && d.Inspeccion.Activo && d.Inspeccion.Estado == EstadoInspeccion.Finalizada
            && (a.VerTodos || a.CentroIds.Contains(d.Inspeccion.CentroId))
            && (a.VerTodos || a.CentroIds.Contains(d.Inspeccion.Vehiculo.CentroId)))
            .OrderBy(d => d.Inspeccion.FechaCreacion).ThenBy(d => d.Id).Take(limite + 1)
            .Select(d => new LecturaAnalitica(d.Id, d.InspeccionId, d.Inspeccion.FechaCreacion, d.PosicionVehiculoId,
                d.PosicionVehiculo.Codigo, d.Inspeccion.Kilometraje, d.ProfundidadExterior, d.ProfundidadCentro, d.ProfundidadInterior)).ToListAsync(ct);
        var tramos = await Asignaciones(new(), a).Where(x => x.LlantaId == id).OrderBy(x => x.FechaInicio).Take(limite + 1)
            .Select(x => new TramoAnalitica(x.Id, x.PosicionVehiculoId, x.FechaInicio, x.FechaFin, x.EsActiva, x.KilometrajeMontaje, x.KilometrajeDesmontaje, x.PosicionVehiculo.EjeVehiculo.Vehiculo.Kilometraje)).ToListAsync(ct);
        if (lecturas.Count > limite || tramos.Count > limite)
            throw new ValidacionException("El historial supera 1.000 lecturas o tramos. Requiere una consulta histórica paginada; no se muestra una serie recortada.");
        var reglas = await db.ParametrosAlerta.AsNoTracking().Where(x => x.Activo && x.Tipo == "PROFUNDIDAD_MINIMA"
            && (!x.CentroId.HasValue || x.CentroId == tire.CentroId)).OrderBy(x => x.Codigo)
            .Select(x => new ReglaProfundidadAnalitica(x.Codigo, x.Operador, x.Valor, x.Unidad, x.CentroId.HasValue ? "Centro actual" : "Global")).ToListAsync(ct);
        var mediciones = DesgasteAnaliticaCalculo.Vincular(lecturas, tramos);
        var limitaciones = new List<string>();
        if (mediciones.Count(x => x.Minima.HasValue && x.TramoId.HasValue) < 3)
            limitaciones.Add("Menos de tres lecturas completas vinculadas a tramos consistentes.");
        if (reglas.Count != 1 || reglas.Any(x => x.Unidad.Trim().ToLowerInvariant() != "mm" || x.Valor < 0 || x.Operador is not ("<" or "<=")))
            limitaciones.Add("No hay un único umbral activo compatible (profundidad mínima en mm, operador < o <=) para el centro actual.");
        limitaciones.Add("El historial no certifica ciclos de reencauche ni continuidad entre tramos. No se unen lecturas de ciclos distintos.");
        limitaciones.Add("La fecha corresponde al registro de la inspección, no a una fecha independiente de toma de lectura. Falta validar continuidad, unidades históricas y calidad de la serie antes de extrapolar.");
        return new(id, tire.Codigo, mediciones.Count, mediciones.Count(x => x.Minima.HasValue), mediciones.Count(x => x.TramoId.HasValue),
            mediciones, reglas, "Datos insuficientes", limitaciones);
    }
}
