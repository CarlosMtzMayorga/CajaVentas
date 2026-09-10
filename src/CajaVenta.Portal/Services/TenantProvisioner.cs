using CajaVenta.Infrastructure;
using CajaVenta.Infrastructure.Persistence;
using CajaVenta.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Services;

public interface ITenantProvisioner
{
    string ProvisionarBaseDatos(Guid identificadorUnico);
    void EliminarBaseDatos(string ruta);
}

public class TenantProvisioner : ITenantProvisioner
{
    private readonly string _carpetaRaizDatos;
    private readonly ILogger<TenantProvisioner> _logger;

    public TenantProvisioner(IWebHostEnvironment environment, ILogger<TenantProvisioner> logger)
    {
        _carpetaRaizDatos = Path.Combine(environment.ContentRootPath, "data", "clientes");
        Directory.CreateDirectory(_carpetaRaizDatos);
        _logger = logger;
    }

    public string ProvisionarBaseDatos(Guid identificadorUnico)
    {
        var archivo = Path.Combine(_carpetaRaizDatos, $"{identificadorUnico:N}.db");
        if (File.Exists(archivo))
            return archivo;

        var conexion = $"Data Source={archivo}";

        var services = new ServiceCollection();
        services.AddInfrastructure(conexion);
        using var proveedor = services.BuildServiceProvider();
        using var scope = proveedor.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<CajaVentaDbContext>();
        context.Database.EnsureCreated();
        EsquemaMigracion.Aplicar(context);
        SeedData.Inicializar(context, scope.ServiceProvider);

        _logger.LogInformation("Base de datos de cliente provisionada: {Archivo}", archivo);
        return archivo;
    }

    public void EliminarBaseDatos(string ruta)
    {
        try
        {
            if (File.Exists(ruta))
                File.Delete(ruta);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo eliminar la base de datos del cliente: {Ruta}", ruta);
        }
    }
}