using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace SistemaLlantas.Infrastructure.Persistence;

// Only constructs metadata for migrations add/script. Does not start API or open a connection.
public sealed class OfflineDesignFactory : IDesignTimeDbContextFactory<LlantasDbContext>
{
    public LlantasDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<LlantasDbContext>()
        .UseSqlServer("Server=offline.invalid;Database=DesignOnly;Integrated Security=True;Encrypt=True").Options);
}
