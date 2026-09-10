using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class VentaConfiguration : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.NumeroTicket)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(v => v.NumeroTicket)
            .IsUnique();

        builder.Property(v => v.Subtotal)
            .HasPrecision(18, 4);

        builder.Property(v => v.Impuestos)
            .HasPrecision(18, 4);

        builder.Property(v => v.Total)
            .HasPrecision(18, 4);

        builder.Property(v => v.MontoRecibido)
            .HasPrecision(18, 4);

        builder.Property(v => v.Cambio)
            .HasPrecision(18, 4);

        builder.Property(v => v.Observaciones)
            .HasMaxLength(500);

        builder.HasIndex(v => v.TurnoCajaId);
        builder.HasIndex(v => v.ClienteId);
        builder.HasIndex(v => v.FechaCreacion);

        builder.HasOne(v => v.TurnoCaja)
            .WithMany(t => t.Ventas)
            .HasForeignKey(v => v.TurnoCajaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(v => v.Cliente)
            .WithMany(c => c.Ventas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
