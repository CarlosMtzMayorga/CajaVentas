using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("Sucursales");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Nombre)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Direccion)
            .HasMaxLength(300);

        builder.Property(s => s.Telefono)
            .HasMaxLength(50);

        builder.Property(s => s.Notas)
            .HasMaxLength(500);

        builder.HasIndex(s => s.Nombre);
    }
}