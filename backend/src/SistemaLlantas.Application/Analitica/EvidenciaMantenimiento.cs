namespace SistemaLlantas.Application.Analitica;

public sealed record EvidenciaMantenimientoEntrada(int AlertasActivas, Guid? AlertaPrincipalId,
    DateTimeOffset? FechaAlertaActivaMasAntigua, MedicionResumen? UltimaLectura, MedicionResumen? UltimaCompleta);
public sealed record EvaluacionMantenimientoV1(Evaluacion<string?> Clasificacion, bool EnCola,
    MotivoMantenimiento? MotivoPrincipal, IReadOnlyList<MotivoMantenimiento> MotivosSecundarios, IReadOnlyList<string> TiposAccion);

public static class EvidenciaMantenimiento
{
    public static EvaluacionMantenimientoV1 Evaluar(EvidenciaMantenimientoEntrada entrada)
    {
        var motivos = new List<MotivoMantenimiento>();
        if (entrada.AlertasActivas > 0)
            motivos.Add(new("ALERTA_ACTIVA", "Alerta abierta o en proceso; severidad no evaluable", entrada.FechaAlertaActivaMasAntigua,
                entrada.AlertaPrincipalId is { } id ? new("ALERTA", id) : null));
        if (entrada.UltimaLectura is null)
            motivos.Add(new("SIN_MEDICION", "Sin medición finalizada en el historial autorizado", null, null));
        else if (!entrada.UltimaLectura.Completa)
        {
            motivos.Add(new("MEDICION_INCOMPLETA", "La última medición finalizada está incompleta o contiene valores negativos",
                entrada.UltimaLectura.FechaRegistro, new("INSPECCION", entrada.UltimaLectura.InspeccionId)));
            if (entrada.UltimaCompleta is { } anterior)
                motivos.Add(new("COMPLETA_ANTERIOR", "Existe una medición completa anterior; no sustituye la última lectura",
                    anterior.FechaRegistro, new("INSPECCION", anterior.InspeccionId)));
        }
        var verificar = entrada.AlertasActivas == 0 && (entrada.UltimaLectura is null || !entrada.UltimaLectura.Completa);
        var clasificacion = verificar
            ? new Evaluacion<string?>("PARCIAL", "CONDICION_POR_VERIFICAR", ["Falta de evidencia observada; vigencia pendiente de configuración"])
            : new Evaluacion<string?>("NO_EVALUABLE", null, ["Severidad y clasificación integral pendientes de reglas de negocio"]);
        var acciones = new List<string>();
        if (entrada.AlertasActivas > 0) acciones.Add("REVISAR_ALERTA");
        if (motivos.Count > 0) { acciones.Add("INSPECCIONAR"); acciones.Add("PROGRAMAR_EVALUACION"); }
        acciones.Add("VER_HISTORIAL");
        return new(clasificacion, motivos.Count > 0, motivos.FirstOrDefault(), motivos.Skip(1).ToArray(), acciones);
    }
}
