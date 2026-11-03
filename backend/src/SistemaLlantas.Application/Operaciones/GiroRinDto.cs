using SistemaLlantas.Application.Common;
namespace SistemaLlantas.Application.Operaciones;

public sealed record GiroRinDto(Guid VehiculoId, Guid PosicionId, Guid LlantaId, decimal? Kilometraje, Guid TecnicoId, DateTimeOffset Fecha, string Motivo)
{
    public void Validar(DateTimeOffset ahora)
    {
        if (VehiculoId == Guid.Empty || PosicionId == Guid.Empty || LlantaId == Guid.Empty || TecnicoId == Guid.Empty)
            throw new ValidacionException("Selecciona vehículo, llanta instalada y técnico.");
        if (!Kilometraje.HasValue || Kilometraje < 0 || Kilometraje > 9999999999999999.99m) throw new ValidacionException("Ingresa un kilometraje válido.");
        if (Fecha == default || Fecha > ahora) throw new ValidacionException("La fecha de intervención es obligatoria y no puede ser futura.");
        if (string.IsNullOrWhiteSpace(Motivo) || Motivo.Trim().Length > 500) throw new ValidacionException("Ingresa un motivo de hasta 500 caracteres.");
    }
}
public sealed record TecnicoGiroDto(Guid Id, string Nombre);
