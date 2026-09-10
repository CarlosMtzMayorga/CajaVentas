using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class StockInventarioConfiguration : IEntityTypeConfiguration<StockInventario>
{
    public void Configure(EntityTypeBuilder<StockInventario> builder)
    {
        builder.ToTable("StocksInventario");

        builder.HasKey(s => new { s.ProductoId, s.SucursalId });

        builder.Property(s => s.StockActual)
            .HasPrecision(18, 4);

        builder.HasOne(s => s.Producto)
            .WithMany(p => p.StocksInventario)
            .HasForeignKey(s => s.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Sucursal)
            .WithMany(suc => suc.StocksInventario)
            .HasForeignKey(s => s.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}