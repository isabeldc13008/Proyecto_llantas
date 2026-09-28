using Microsoft.AspNetCore.Mvc.Routing;
using SistemaLlantas.Api.Controllers;

namespace SistemaLlantas.Api.IntegrationTests;

public sealed class ServiciosLlantaContractTests
{
    [Fact]
    public void PartialesConservanRutasDeServiciosYDisposicionSinDuplicarlas()
    {
        var rutas = typeof(ServiciosLlantaController).GetMethods()
            .SelectMany(m => m.GetCustomAttributes(typeof(HttpMethodAttribute), true).Cast<HttpMethodAttribute>())
            .SelectMany(a => a.HttpMethods.Select(verb => $"{verb} {a.Template ?? ""}".TrimEnd())).ToArray();
        Assert.Equal(rutas.Length, rutas.Distinct(StringComparer.OrdinalIgnoreCase).Count());
        foreach (var ruta in new[] { "GET", "POST", "GET reparaciones", "POST reparaciones/lotes",
            "POST {id:guid}/aprobar", "POST {id:guid}/evaluar-disposicion", "GET disposicion/lotes",
            "POST disposicion/lotes", "POST disposicion/lotes/{id:guid}/recibir", "POST disposicion/lotes/{id:guid}/cerrar" })
            Assert.Contains(ruta, rutas);
    }
}
