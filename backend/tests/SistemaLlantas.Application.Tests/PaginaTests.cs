using SistemaLlantas.Application.Common;

namespace SistemaLlantas.Application.Tests;

public sealed class PaginaTests
{
    [Fact] public void CalculaTotalPaginas() => Assert.Equal(3, new Pagina<int>([], 1, 20, 41).TotalPages);
    [Fact] public void LimitaTamanoConsulta() => Assert.Equal(100, new ConsultaPaginada(PageSize: 500).Tamano);
}
