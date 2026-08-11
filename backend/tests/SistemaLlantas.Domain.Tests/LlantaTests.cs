using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Domain.Tests;

public sealed class LlantaTests
{
    [Fact]
    public void Constructor_NormalizaCodigoYSerial()
    {
        var llanta = new Llanta("  ll-001 ", " serial-9 ");
        Assert.Equal("LL-001", llanta.Codigo); Assert.Equal("SERIAL-9", llanta.Serial);
    }

    [Theory]
    [InlineData("", "SERIAL")]
    [InlineData("CODIGO", " ")]
    public void Constructor_RechazaIdentificacionVacia(string codigo, string serial) => Assert.Throws<ArgumentException>(() => new Llanta(codigo, serial));
}
