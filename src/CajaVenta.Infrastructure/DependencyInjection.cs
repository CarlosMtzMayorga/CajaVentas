using CajaVenta.Application.Common;
using CajaVenta.Application.Interfaces;
using CajaVenta.Application.Services;
using CajaVenta.Domain.Interfaces;
using CajaVenta.Infrastructure.Persistence;
using CajaVenta.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CajaVenta.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CajaVentaDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<ITurnoCajaRepository, TurnoCajaRepository>();
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<ISucursalRepository, SucursalRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolPermisoRepository, RolPermisoRepository>();
        services.AddScoped<ICorteZRepository, CorteZRepository>();

        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IVentaService, VentaService>();
        services.AddScoped<ITurnoService, TurnoService>();
        services.AddScoped<IInventarioService, InventarioService>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<ISucursalService, SucursalService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IPermisoService, PermisoService>();
        services.AddScoped<ICorteZService, CorteZService>();

        return services;
    }
}
