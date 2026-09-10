using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.CodigoBarras)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(p => p.CodigoBarras)
            .IsUnique();

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Descripcion)
            .HasMaxLength(500);

        builder.Property(p => p.Costo)
            .HasPrecision(18, 4);

        builder.Property(p => p.PrecioVenta)
            .HasPrecision(18, 4);

        builder.Property(p => p.StockMinimo)
            .HasPrecision(18, 4);

        builder.Property(p => p.Categoria)
            .HasMaxLength(100);

        builder.Property(p => p.UnidadMedida)
            .HasMaxLength(50);

        builder.HasIndex(p => p.Nombre);
        builder.HasIndex(p => p.Categoria);
    }
}
