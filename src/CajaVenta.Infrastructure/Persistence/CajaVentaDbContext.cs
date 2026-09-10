using CajaVenta.Domain.Common;
using CajaVenta.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Infrastructure.Persistence;

public class CajaVentaDbContext : DbContext
{
    public CajaVentaDbContext(DbContextOptions<CajaVentaDbContext> options) : base(options) { }

    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<VariantesProducto> VariantesProducto => Set<VariantesProducto>();
    public DbSet<MovimientoInventario> MovimientosInventario => Set<MovimientoInventario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();
    public DbSet<TurnoCaja> TurnosCaja => Set<TurnoCaja>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Sucursal> Sucursales => Set<Sucursal>();
    public DbSet<Caja> Cajas => Set<Caja>();
    public DbSet<StockInventario> StocksInventario => Set<StockInventario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CajaVentaDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.FechaModificacion = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
