using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class CorteZConfiguration : IEntityTypeConfiguration<CorteZ>
{
    public void Configure(EntityTypeBuilder<CorteZ> builder)
    {
        builder.ToTable("CortesZ");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Numero)
            .IsRequired();

        builder.HasIndex(c => new { c.CajaId, c.Numero })
            .IsUnique();

        builder.Property(c => c.FondoInicial)
            .HasPrecision(18, 4);

        builder.Property(c => c.FondoFinal)
            .HasPrecision(18, 4);

        builder.Property(c => c.Subtotal)
            .HasPrecision(18, 4);

        builder.Property(c => c.Impuestos)
            .HasPrecision(18, 4);

        builder.Property(c => c.TotalVentas)
            .HasPrecision(18, 4);

        builder.Property(c => c.VentasEfectivo)
            .HasPrecision(18, 4);

        builder.Property(c => c.VentasTarjeta)
            .HasPrecision(18, 4);

        builder.Property(c => c.VentasOtros)
            .HasPrecision(18, 4);

        builder.HasIndex(c => c.SucursalId);
        builder.HasIndex(c => c.CajaId);
        builder.HasIndex(c => c.TurnoCajaId);

        builder.HasOne(c => c.TurnoCaja)
            .WithMany()
            .HasForeignKey(c => c.TurnoCajaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Sucursal)
            .WithMany()
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Caja)
            .WithMany()
            .HasForeignKey(c => c.CajaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Usuario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}