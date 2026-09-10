using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class VariantesProductoConfiguration : IEntityTypeConfiguration<VariantesProducto>
{
    public void Configure(EntityTypeBuilder<VariantesProducto> builder)
    {
        builder.ToTable("VariantesProducto");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Talla)
            .HasMaxLength(50);

        builder.Property(v => v.Color)
            .HasMaxLength(50);

        builder.Property(v => v.Presentacion)
            .HasMaxLength(100);

        builder.Property(v => v.SKU)
            .HasMaxLength(50);

        builder.Property(v => v.PrecioAdicional)
            .HasPrecision(18, 4);

        builder.Property(v => v.StockActual)
            .HasPrecision(18, 4);

        builder.HasIndex(v => v.SKU)
            .IsUnique()
            .HasFilter("[SKU] IS NOT NULL");

        builder.HasOne(v => v.Producto)
            .WithMany(p => p.Variantes)
            .HasForeignKey(v => v.ProductoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
