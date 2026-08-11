using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaLlantas.Domain.Entities;

namespace SistemaLlantas.Infrastructure.Persistence;

public sealed class LlantaConfiguration : IEntityTypeConfiguration<Llanta>
{
    public void Configure(EntityTypeBuilder<Llanta> b)
    {
        b.ToTable("TBL_Llanta"); b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
        b.Property(x => x.Serial).HasMaxLength(100).IsRequired();
        b.Property(x => x.UbicacionActual).HasMaxLength(150).IsRequired();
        b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.Property(x => x.Costo).HasPrecision(18, 2);
        b.Property(x => x.ProfundidadInicial).HasPrecision(8, 2);
        b.Property(x => x.KilometrajeAcumulado).HasPrecision(18, 2);
        b.HasIndex(x => x.Codigo).IsUnique().HasDatabaseName("IX_Llanta_Codigo");
        b.HasIndex(x => x.Serial).IsUnique().HasDatabaseName("IX_Llanta_Serial");
        b.HasIndex(x => new { x.CentroId, x.EstadoLlantaId }).HasDatabaseName("IX_Llanta_CentroEstado");
        b.HasOne(x => x.Marca).WithMany().HasForeignKey(x => x.MarcaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Referencia).WithMany().HasForeignKey(x => x.ReferenciaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Dimension).WithMany().HasForeignKey(x => x.DimensionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TipoLlanta).WithMany().HasForeignKey(x => x.TipoLlantaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EstadoLlanta).WithMany().HasForeignKey(x => x.EstadoLlantaId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Centro).WithMany().HasForeignKey(x => x.CentroId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CatalogosConfiguration :
    IEntityTypeConfiguration<Marca>, IEntityTypeConfiguration<Referencia>, IEntityTypeConfiguration<Dimension>,
    IEntityTypeConfiguration<TipoLlanta>, IEntityTypeConfiguration<EstadoLlanta>, IEntityTypeConfiguration<Centro>
{
    public void Configure(EntityTypeBuilder<Marca> b) { Base(b, "Marca"); }
    public void Configure(EntityTypeBuilder<Referencia> b) { Base(b, "Referencia"); b.HasOne(x => x.Marca).WithMany(x => x.Referencias).HasForeignKey(x => x.MarcaId).OnDelete(DeleteBehavior.Restrict); }
    public void Configure(EntityTypeBuilder<Dimension> b) => Base(b, "Dimension");
    public void Configure(EntityTypeBuilder<TipoLlanta> b) => Base(b, "TipoLlanta");
    public void Configure(EntityTypeBuilder<EstadoLlanta> b) => Base(b, "EstadoLlanta");
    public void Configure(EntityTypeBuilder<Centro> b) => Base(b, "Centro");
    private static void Base<T>(EntityTypeBuilder<T> b, string nombre) where T : CatalogoBase
    {
        b.ToTable($"TBL_{nombre}"); b.HasKey(x => x.Id);
        b.Property(x => x.Codigo).HasMaxLength(30).IsRequired(); b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.HasIndex(x => x.Codigo).IsUnique().HasDatabaseName($"IX_{nombre}_Codigo");
    }
}

public sealed class AuditoriaConfiguration : IEntityTypeConfiguration<Auditoria>
{
    public void Configure(EntityTypeBuilder<Auditoria> b)
    {
        b.ToTable("TBL_Auditoria"); b.HasKey(x => x.Id); b.Property(x => x.Usuario).HasMaxLength(150);
        b.Property(x => x.Accion).HasMaxLength(30); b.Property(x => x.Entidad).HasMaxLength(150);
        b.Property(x => x.Identificador).HasMaxLength(100); b.HasIndex(x => new { x.Entidad, x.Identificador, x.Fecha }).HasDatabaseName("IX_Auditoria_EntidadFecha");
    }
}
