using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class MovimientoInventarioConfiguration : IEntityTypeConfiguration<MovimientoInventario>
{
    public void Configure(EntityTypeBuilder<MovimientoInventario> builder)
    {
        builder.ToTable("MovimientosInventario");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Cantidad)
            .HasPrecision(18, 4);

        builder.Property(m => m.CostoUnitario)
            .HasPrecision(18, 4);

        builder.Property(m => m.Referencia)
            .HasMaxLength(100);

        builder.Property(m => m.Observaciones)
            .HasMaxLength(500);

        builder.HasIndex(m => m.ProductoId);
        builder.HasIndex(m => m.SucursalId);
        builder.HasIndex(m => m.Fecha);

        builder.HasOne(m => m.Producto)
            .WithMany(p => p.MovimientosInventario)
            .HasForeignKey(m => m.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Sucursal)
            .WithMany(s => s.MovimientosInventario)
            .HasForeignKey(m => m.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
