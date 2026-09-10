using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class TurnoCajaConfiguration : IEntityTypeConfiguration<TurnoCaja>
{
    public void Configure(EntityTypeBuilder<TurnoCaja> builder)
    {
        builder.ToTable("TurnosCaja");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.FondoInicial)
            .HasPrecision(18, 4);

        builder.Property(t => t.FondoFinal)
            .HasPrecision(18, 4);

        builder.Property(t => t.VentasEfectivo)
            .HasPrecision(18, 4);

        builder.Property(t => t.TotalVentas)
            .HasPrecision(18, 4);

        builder.Property(t => t.ObservacionesApertura)
            .HasMaxLength(500);

        builder.Property(t => t.ObservacionesCierre)
            .HasMaxLength(500);

        builder.HasIndex(t => t.UsuarioId);
        builder.HasIndex(t => t.SucursalId);
        builder.HasIndex(t => t.CajaId);
        builder.HasIndex(t => t.Estado);

        builder.HasOne(t => t.Usuario)
            .WithMany(u => u.Turnos)
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Sucursal)
            .WithMany(s => s.Turnos)
            .HasForeignKey(t => t.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Caja)
            .WithMany(c => c.Turnos)
            .HasForeignKey(t => t.CajaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
