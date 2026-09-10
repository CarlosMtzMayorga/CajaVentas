using CajaVenta.Portal.Data;
using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Services;

public interface ISuscriptorService
{
    Task<List<Suscriptor>> ObtenerTodosAsync();
    Task<Suscriptor?> ObtenerPorIdAsync(Guid id);
    Task<Suscriptor?> ObtenerPorUrlAsync(string url);
    Task<ResultadoOperacion> CrearAsync(string nombreComercial, string? razonSocial, string? rfc,
        string? email, string? telefono, string urlAcceso, EstadoSuscriptor estado);
    Task<ResultadoOperacion> ActualizarAsync(Guid id, string nombreComercial, string? razonSocial,
        string? rfc, string? email, string? telefono, string urlAcceso, EstadoSuscriptor estado);
    Task<ResultadoOperacion> EliminarAsync(Guid id);
    Task<ResultadoOperacion> CambiarEstadoAsync(Guid id, EstadoSuscriptor nuevoEstado);
}

public class ResultadoOperacion
{
    public bool EsExitoso { get; set; }
    public string? Error { get; set; }
    public Guid? Id { get; set; }
    public static ResultadoOperacion Exito(Guid? id = null) =>
        new() { EsExitoso = true, Id = id };
    public static ResultadoOperacion Fallo(string error) =>
        new() { EsExitoso = false, Error = error };
}

public class SuscriptorService : ISuscriptorService
{
    private readonly PortalDbContext _context;
    private readonly ITenantProvisioner _provisioner;

    public SuscriptorService(PortalDbContext context, ITenantProvisioner provisioner)
    {
        _context = context;
        _provisioner = provisioner;
    }

    public Task<List<Suscriptor>> ObtenerTodosAsync() =>
        _context.Suscriptores.AsNoTracking().OrderBy(s => s.NombreComercial).ToListAsync();

    public Task<Suscriptor?> ObtenerPorIdAsync(Guid id) =>
        _context.Suscriptores
            .Include(s => s.Suscripciones).ThenInclude(s => s.Plan)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<Suscriptor?> ObtenerPorUrlAsync(string url) =>
        _context.Suscriptores.AsNoTracking().FirstOrDefaultAsync(s => s.UrlAcceso == url);

    public async Task<ResultadoOperacion> CrearAsync(string nombreComercial, string? razonSocial, string? rfc,
        string? email, string? telefono, string urlAcceso, EstadoSuscriptor estado)
    {
        if (string.IsNullOrWhiteSpace(nombreComercial))
            return ResultadoOperacion.Fallo("El nombre comercial es obligatorio.");

        urlAcceso = (urlAcceso ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(urlAcceso))
            return ResultadoOperacion.Fallo("La URL de acceso es obligatoria.");

        if (await _context.Suscriptores.AnyAsync(s => s.UrlAcceso == urlAcceso))
            return ResultadoOperacion.Fallo($"Ya existe un cliente con la URL '{urlAcceso}'.");

        var id = Guid.NewGuid();
        string rutaBaseDatos;
        try
        {
            rutaBaseDatos = _provisioner.ProvisionarBaseDatos(id);
        }
        catch (Exception ex)
        {
            return ResultadoOperacion.Fallo($"No se pudo crear la base de datos del cliente: {ex.Message}");
        }

        var suscriptor = new Suscriptor
        {
            Id = id,
            NombreComercial = nombreComercial.Trim(),
            RazonSocial = razonSocial,
            Rfc = rfc,
            Email = email,
            Telefono = telefono,
            UrlAcceso = urlAcceso,
            RutaBaseDatos = rutaBaseDatos,
            Estado = estado,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Suscriptores.Add(suscriptor);
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(suscriptor.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Guid id, string nombreComercial, string? razonSocial,
        string? rfc, string? email, string? telefono, string urlAcceso, EstadoSuscriptor estado)
    {
        var suscriptor = await _context.Suscriptores.FindAsync(id);
        if (suscriptor is null)
            return ResultadoOperacion.Fallo("Cliente no encontrado.");

        urlAcceso = (urlAcceso ?? "").Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(urlAcceso))
            return ResultadoOperacion.Fallo("La URL de acceso es obligatoria.");

        if (await _context.Suscriptores.AnyAsync(s => s.UrlAcceso == urlAcceso && s.Id != id))
            return ResultadoOperacion.Fallo($"Ya existe un cliente con la URL '{urlAcceso}'.");

        suscriptor.NombreComercial = nombreComercial.Trim();
        suscriptor.RazonSocial = razonSocial;
        suscriptor.Rfc = rfc;
        suscriptor.Email = email;
        suscriptor.Telefono = telefono;
        suscriptor.UrlAcceso = urlAcceso;
        suscriptor.Estado = estado;
        suscriptor.FechaModificacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }

    public async Task<ResultadoOperacion> EliminarAsync(Guid id)
    {
        var suscriptor = await _context.Suscriptores
            .Include(s => s.Suscripciones)
            .FirstOrDefaultAsync(s => s.Id == id);
        if (suscriptor is null)
            return ResultadoOperacion.Fallo("Cliente no encontrado.");

        if (suscriptor.Suscripciones.Any(s => s.Estado == EstadoSuscripcion.Activa))
            return ResultadoOperacion.Fallo("No se puede eliminar un cliente con una suscripción activa.");

        _context.Suscriptores.Remove(suscriptor);
        await _context.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(suscriptor.RutaBaseDatos))
            _provisioner.EliminarBaseDatos(suscriptor.RutaBaseDatos);

        return ResultadoOperacion.Exito();
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(Guid id, EstadoSuscriptor nuevoEstado)
    {
        var suscriptor = await _context.Suscriptores.FindAsync(id);
        if (suscriptor is null)
            return ResultadoOperacion.Fallo("Cliente no encontrado.");

        suscriptor.Estado = nuevoEstado;
        suscriptor.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }
}