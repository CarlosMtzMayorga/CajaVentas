using CajaVenta.Portal.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CajaVenta.Portal.Data;

public class SuscriptorConfiguration : IEntityTypeConfiguration<Suscriptor>
{
    public void Configure(EntityTypeBuilder<Suscriptor> builder)
    {
        builder.ToTable("Suscriptores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.NombreComercial).IsRequired().HasMaxLength(200);
        builder.Property(s => s.RazonSocial).HasMaxLength(250);
        builder.Property(s => s.Rfc).HasMaxLength(20);
        builder.Property(s => s.Email).HasMaxLength(150);
        builder.Property(s => s.Telefono).HasMaxLength(30);
        builder.Property(s => s.UrlAcceso).HasMaxLength(300);
        builder.Property(s => s.RutaBaseDatos).HasMaxLength(500);
        builder.HasIndex(s => s.UrlAcceso).IsUnique();
    }
}

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("Planes");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Descripcion).HasMaxLength(400);
        builder.Property(p => p.PrecioMensual).HasPrecision(18, 2);
    }
}

public class SuscripcionConfiguration : IEntityTypeConfiguration<Suscripcion>
{
    public void Configure(EntityTypeBuilder<Suscripcion> builder)
    {
        builder.ToTable("Suscripciones");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PrecioMensual).HasPrecision(18, 2);

        builder.HasOne(s => s.Suscriptor)
            .WithMany(s => s.Suscripciones)
            .HasForeignKey(s => s.SuscriptorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.SuscriptorId);
        builder.HasIndex(s => s.Estado);
    }
}

public class PagoConfiguration : IEntityTypeConfiguration<Pago>
{
    public void Configure(EntityTypeBuilder<Pago> builder)
    {
        builder.ToTable("Pagos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Monto).HasPrecision(18, 2);
        builder.Property(p => p.ReferenciaExterna).HasMaxLength(200);
        builder.Property(p => p.Notas).HasMaxLength(500);
        builder.HasIndex(p => p.SuscriptorId);
        builder.HasIndex(p => p.SuscripcionId);

        builder.HasOne(p => p.Suscriptor)
            .WithMany()
            .HasForeignKey(p => p.SuscriptorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Suscripcion)
            .WithMany()
            .HasForeignKey(p => p.SuscripcionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class UsuarioPortalConfiguration : IEntityTypeConfiguration<UsuarioPortal>
{
    public void Configure(EntityTypeBuilder<UsuarioPortal> builder)
    {
        builder.ToTable("UsuariosPortal");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.NombreUsuario).IsRequired().HasMaxLength(80);
        builder.Property(u => u.ContrasenaHash).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Nombre).HasMaxLength(160);
        builder.Property(u => u.Rol).HasMaxLength(30);
        builder.HasIndex(u => u.NombreUsuario).IsUnique();
    }
}