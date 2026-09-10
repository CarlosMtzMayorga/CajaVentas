using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.NombreUsuario)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(u => u.NombreUsuario)
            .IsUnique();

        builder.Property(u => u.ContrasenaHash)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.Email)
            .HasMaxLength(200);

        builder.Property(u => u.Rol)
            .HasMaxLength(50);

        builder.HasIndex(u => u.SucursalId);

        builder.HasOne(u => u.Sucursal)
            .WithMany(s => s.Usuarios)
            .HasForeignKey(u => u.SucursalId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
