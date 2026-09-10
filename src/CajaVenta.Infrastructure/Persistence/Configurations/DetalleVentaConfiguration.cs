using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class DetalleVentaConfiguration : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("DetallesVenta");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Cantidad)
            .HasPrecision(18, 4);

        builder.Property(d => d.PrecioUnitario)
            .HasPrecision(18, 4);

        builder.Property(d => d.Subtotal)
            .HasPrecision(18, 4);

        builder.Property(d => d.Impuesto)
            .HasPrecision(18, 4);

        builder.Property(d => d.Total)
            .HasPrecision(18, 4);

        builder.HasIndex(d => d.VentaId);
        builder.HasIndex(d => d.ProductoId);

        builder.HasOne(d => d.Venta)
            .WithMany(v => v.Detalles)
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Producto)
            .WithMany(p => p.DetallesVenta)
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Variante)
            .WithMany()
            .HasForeignKey(d => d.VarianteId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
