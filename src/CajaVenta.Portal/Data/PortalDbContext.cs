using CajaVenta.Portal.Entities;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Data;

public class PortalDbContext : DbContext
{
    public PortalDbContext(DbContextOptions<PortalDbContext> options) : base(options) { }

    public DbSet<Suscriptor> Suscriptores => Set<Suscriptor>();
    public DbSet<Plan> Planes => Set<Plan>();
    public DbSet<Suscripcion> Suscripciones => Set<Suscripcion>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<UsuarioPortal> UsuariosPortal => Set<UsuarioPortal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PortalDbContext).Assembly);
    }
}