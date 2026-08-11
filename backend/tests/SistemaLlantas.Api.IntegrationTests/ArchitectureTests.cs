namespace SistemaLlantas.Api.IntegrationTests;

public sealed class ArchitectureTests
{
    [Fact]
    public void ApiAssembly_IsLoadable() => Assert.NotNull(typeof(Program).Assembly);
}
