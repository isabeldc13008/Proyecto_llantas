using Microsoft.EntityFrameworkCore;
using SistemaLlantas.Infrastructure.Persistence;
using SistemaLlantas.Infrastructure.Services;
namespace SistemaLlantas.Api.IntegrationTests;
public sealed class MountReservationQueryTests
{
 [Fact]public void DisponibilidadTraduceReservasYExcluyeSoloElGrupoPropio()
 {
  using var db=new LlantasDbContext(new DbContextOptionsBuilder<LlantasDbContext>().UseSqlServer("Server=localhost;Database=TranslationOnly;Integrated Security=True;TrustServerCertificate=True").Options);
  var group=Guid.NewGuid();var sql=LlantasDisponibles.Consulta(db,group).ToQueryString();
  Assert.Contains("GGrupoOperacionId",sql);Assert.Contains("GGrupoProgramacionId",sql);Assert.Contains("NOT EXISTS",sql);Assert.Contains("OrdenServicio",sql);Assert.Contains(group.ToString(),sql);
 }
}
