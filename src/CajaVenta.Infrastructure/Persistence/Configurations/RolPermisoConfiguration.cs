using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class RolPermisoConfiguration : IEntityTypeConfiguration<RolPermiso>
{
    public void Configure(EntityTypeBuilder<RolPermiso> builder)
    {
        builder.ToTable("RolesPermisos");

        builder.HasKey(rp => new { rp.Rol, rp.PermisoKey });

        builder.Property(rp => rp.Rol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(rp => rp.PermisoKey)
            .IsRequired()
            .HasMaxLength(60);

        builder.HasIndex(rp => rp.Rol);
    }
}