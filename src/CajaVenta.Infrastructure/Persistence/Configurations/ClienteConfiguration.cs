using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NombreRazonSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.NombreComercial)
            .HasMaxLength(200);

        builder.Property(c => c.RFC)
            .IsRequired()
            .HasMaxLength(13);

        builder.HasIndex(c => c.RFC)
            .IsUnique();

        builder.Property(c => c.Email)
            .HasMaxLength(150);

        builder.Property(c => c.Telefono)
            .HasMaxLength(30);

        builder.Property(c => c.CodigoPostal)
            .HasMaxLength(10);

        builder.Property(c => c.RegimenFiscal)
            .HasMaxLength(100);

        builder.Property(c => c.UsoCFDI)
            .HasMaxLength(10);

        builder.Property(c => c.ConstanciaRuta)
            .HasMaxLength(500);

        builder.Property(c => c.ConstanciaOriginalNombre)
            .HasMaxLength(255);

        builder.Property(c => c.Notas)
            .HasMaxLength(1000);
    }
}