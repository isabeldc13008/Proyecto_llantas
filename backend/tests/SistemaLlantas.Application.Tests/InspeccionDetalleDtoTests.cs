using System.ComponentModel.DataAnnotations;
using SistemaLlantas.Application.Inspecciones;

namespace SistemaLlantas.Application.Tests;

public sealed class InspeccionDetalleDtoTests
{
    public static TheoryData<string> Profundidades => new()
    {
        nameof(GuardarDetalleInspeccionDto.ProfundidadExterior),
        nameof(GuardarDetalleInspeccionDto.ProfundidadCentro),
        nameof(GuardarDetalleInspeccionDto.ProfundidadInterior)
    };

    [Theory]
    [MemberData(nameof(Profundidades))]
    public void Profundidad_Acepta25(string propiedad) => Assert.True(EsValida(propiedad, 25m));

    [Theory]
    [MemberData(nameof(Profundidades))]
    public void Profundidad_RechazaMayorA25(string propiedad) => Assert.False(EsValida(propiedad, 25.5m));

    [Theory]
    [MemberData(nameof(Profundidades))]
    public void Profundidad_RechazaNegativa(string propiedad) => Assert.False(EsValida(propiedad, -0.5m));

    [Theory]
    [MemberData(nameof(Profundidades))]
    public void Profundidad_PermiteNula(string propiedad) => Assert.True(EsValida(propiedad, null));

    private static bool EsValida(string propiedad, decimal? valor)
    {
        var dto = new GuardarDetalleInspeccionDto();
        typeof(GuardarDetalleInspeccionDto).GetProperty(propiedad)!.SetValue(dto, valor);
        return Validator.TryValidateObject(dto, new ValidationContext(dto), [], true);
    }
}
