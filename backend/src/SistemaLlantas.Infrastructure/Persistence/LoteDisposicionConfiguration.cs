using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaLlantas.Domain.Entities;
namespace SistemaLlantas.Infrastructure.Persistence;
public sealed class LoteDisposicionConfiguration:IEntityTypeConfiguration<LoteDisposicionFinal>,IEntityTypeConfiguration<OrdenServicioLlanta>
{
 public void Configure(EntityTypeBuilder<LoteDisposicionFinal> b)
 {
  b.ToTable("TBL_LoteDisposicionFinal","dbo");b.HasKey(x=>x.Id);
  b.Property(x=>x.Id).HasColumnName("GId");b.Property(x=>x.Codigo).HasColumnName("SCodigo").HasMaxLength(30);
  b.Property(x=>x.CentroOrigenId).HasColumnName("GCentroOrigenId");b.Property(x=>x.CentroDestinoId).HasColumnName("GCentroDestinoId");
  b.Property(x=>x.RelevanciaDestino).HasColumnName("SRelevanciaDestino").HasMaxLength(2);
  b.Property(x=>x.Estado).HasColumnName("SEstado").HasMaxLength(40);
  b.Property(x=>x.Placa).HasColumnName("SPlaca").HasMaxLength(20);
  b.Property(x=>x.FechaSalida).HasColumnName("DFechaSalida");b.Property(x=>x.FechaRecepcion).HasColumnName("DFechaRecepcion");b.Property(x=>x.FechaCierre).HasColumnName("DFechaCierre");
  b.Property(x=>x.Remision).HasColumnName("SRemision").HasMaxLength(100);b.Property(x=>x.Transportador).HasColumnName("STransportador").HasMaxLength(150);b.Property(x=>x.Observaciones).HasColumnName("SObservaciones").HasMaxLength(1000);
  b.Property(x=>x.Receptor).HasColumnName("SReceptor").HasMaxLength(150);b.Property(x=>x.IdempotencyKey).HasColumnName("SIdempotencyKey").HasMaxLength(100);
  b.Property(x=>x.Activo).HasColumnName("BActivo");b.Property(x=>x.FechaCreacion).HasColumnName("DFechaCreacion");b.Property(x=>x.FechaModificacion).HasColumnName("DFechaModificacion");b.Property(x=>x.UsuarioCreacion).HasColumnName("SUsuarioCreacion");b.Property(x=>x.UsuarioModificacion).HasColumnName("SUsuarioModificacion");b.Property(x=>x.RowVersion).HasColumnName("TRowVersion");
  b.HasIndex(x=>x.Codigo).IsUnique();b.HasIndex(x=>x.IdempotencyKey).IsUnique();
  b.HasOne(x=>x.CentroOrigen).WithMany().HasForeignKey(x=>x.CentroOrigenId).OnDelete(DeleteBehavior.Restrict);
  b.HasOne(x=>x.CentroDestino).WithMany().HasForeignKey(x=>x.CentroDestinoId).OnDelete(DeleteBehavior.Restrict);
 }
 public void Configure(EntityTypeBuilder<OrdenServicioLlanta> b)
 {
  b.Property(x=>x.LoteDisposicionFinalId).HasColumnName("GLoteDisposicionFinalId");
  b.HasOne(x=>x.LoteDisposicionFinal).WithMany(x=>x.Ordenes).HasForeignKey(x=>x.LoteDisposicionFinalId).OnDelete(DeleteBehavior.Restrict);
  b.Property(x=>x.EvaluadoPor).HasColumnName("SEvaluadoPor").HasMaxLength(150);
  b.Property(x=>x.FechaEvaluacion).HasColumnName("DFechaEvaluacion");b.Property(x=>x.FechaDisposicion).HasColumnName("DFechaDisposicion");
 }
}
