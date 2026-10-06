using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaLlantas.Domain.Common;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Persistence;

public sealed class DespachoDisposicionConfiguration :
    IEntityTypeConfiguration<DespachoDisposicion>, IEntityTypeConfiguration<DespachoDisposicionItem>,
    IEntityTypeConfiguration<ActaDisposicion>, IEntityTypeConfiguration<SoporteActaDisposicion>, IEntityTypeConfiguration<NovedadDisposicion>
{
    private static void Audit<T>(EntityTypeBuilder<T> b, string table) where T : EntidadAuditable
    {
        b.ToTable(table, "dbo"); b.HasKey(x => x.Id);
        b.Property(x => x.UsuarioCreacion).HasMaxLength(150);
        b.Property(x => x.UsuarioModificacion).HasMaxLength(150);
        b.Property(x => x.RowVersion).IsRowVersion();
    }
    public void Configure(EntityTypeBuilder<DespachoDisposicion> b)
    {
        Audit(b, "TBL_DespachoDisposicion");
        b.Property(x => x.Codigo).HasMaxLength(50); b.HasIndex(x => x.Codigo).IsUnique();
        b.Property(x => x.Transportador).HasMaxLength(150); b.Property(x => x.Placa).HasMaxLength(20);
        b.Property(x => x.Remision).HasMaxLength(100); b.Property(x => x.Observaciones).HasMaxLength(1000);
        b.Property(x => x.Estado).HasMaxLength(30); b.Property(x => x.CerradoPor).HasMaxLength(150);
        b.Property(x => x.IdempotencyKey).HasMaxLength(100); b.HasIndex(x => x.IdempotencyKey).IsUnique();
        b.Property(x => x.SolicitudHash).HasMaxLength(64);
        b.HasOne(x => x.CentroR1).WithMany().HasForeignKey(x => x.CentroR1Id).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Proveedor).WithMany().HasForeignKey(x => x.ProveedorId).OnDelete(DeleteBehavior.Restrict);
    }
    public void Configure(EntityTypeBuilder<DespachoDisposicionItem> b)
    {
        Audit(b, "TBL_DespachoDisposicionItem");
        b.HasOne(x => x.Despacho).WithMany(x => x.Items).HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Orden).WithMany().HasForeignKey(x => x.OrdenId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.LoteEntrada).WithMany().HasForeignKey(x => x.LoteEntradaId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.OrdenId).IsUnique().HasFilter("[Activo] = 1");
    }
    public void Configure(EntityTypeBuilder<ActaDisposicion> b)
    {
        Audit(b, "TBL_ActaDisposicion");
        b.Property(x => x.Codigo).HasMaxLength(50); b.HasIndex(x => x.Codigo).IsUnique();
        b.HasOne(x => x.Despacho).WithMany(x => x.Actas).HasForeignKey(x => x.DespachoId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CentroOrigen).WithMany().HasForeignKey(x => x.CentroOrigenId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.DespachoId, x.CentroOrigenId }).IsUnique();
    }
    public void Configure(EntityTypeBuilder<SoporteActaDisposicion> b)
    {
        Audit(b, "TBL_SoporteActaDisposicion");
        b.Property(x => x.NombreArchivo).HasMaxLength(255); b.Property(x => x.MimeType).HasMaxLength(100);
        b.Property(x => x.Hash).HasMaxLength(64); b.Property(x => x.Ubicacion).HasMaxLength(500);
        b.HasOne(x => x.Acta).WithMany(x => x.Soportes).HasForeignKey(x => x.ActaId).OnDelete(DeleteBehavior.Restrict);
    }
    public void Configure(EntityTypeBuilder<NovedadDisposicion> b)
    {
        Audit(b, "TBL_NovedadDisposicion");
        b.Property(x => x.Observacion).HasMaxLength(1000); b.Property(x => x.Resolucion).HasMaxLength(1000);
        b.Property(x => x.ResueltaPor).HasMaxLength(150);
        b.HasOne(x => x.Lote).WithMany().HasForeignKey(x => x.LoteId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Orden).WithMany().HasForeignKey(x => x.OrdenId).OnDelete(DeleteBehavior.Restrict);
    }
}
