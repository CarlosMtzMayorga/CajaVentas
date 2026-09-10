using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class CajaConfiguration : IEntityTypeConfiguration<Caja>
{
    public void Configure(EntityTypeBuilder<Caja> builder)
    {
        builder.ToTable("Cajas");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.Notas)
            .HasMaxLength(500);

        builder.HasIndex(c => c.SucursalId);

        builder.HasOne(c => c.Sucursal)
            .WithMany(s => s.Cajas)
            .HasForeignKey(c => c.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}