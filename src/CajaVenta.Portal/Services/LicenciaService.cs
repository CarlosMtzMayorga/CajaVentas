using CajaVenta.Portal.Data;
using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Services;

public interface ILicenciaService
{
    Task<LicenciaDto?> ObtenerLicenciaAsync(string urlAcceso);
}

public class LicenciaDto
{
    public bool Existe { get; set; }
    public bool EstaActiva { get; set; }
    public string? NombreComercial { get; set; }
    public string? Plan { get; set; }
    public DateTime? Vence { get; set; }
    public DateTime? UltimoPago { get; set; }
    public EstadoSuscriptor? EstadoCliente { get; set; }
    public EstadoSuscripcion? EstadoSuscripcion { get; set; }
}

public class LicenciaService : ILicenciaService
{
    private readonly PortalDbContext _context;

    public LicenciaService(PortalDbContext context) => _context = context;

    public async Task<LicenciaDto?> ObtenerLicenciaAsync(string urlAcceso)
    {
        var suscriptor = await _context.Suscriptores.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UrlAcceso == urlAcceso);

        if (suscriptor is null)
            return null;

        var suscripcion = await _context.Suscripciones.AsNoTracking()
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.SuscriptorId == suscriptor.Id && s.Estado == EstadoSuscripcion.Activa);

        var ultimoPago = await _context.Pagos.AsNoTracking()
            .Where(p => p.SuscriptorId == suscriptor.Id && p.Estado == EstadoPago.Pagado)
            .OrderByDescending(p => p.Fecha)
            .FirstOrDefaultAsync();

        var clienteActivo = suscriptor.Estado == EstadoSuscriptor.Activo || suscriptor.Estado == EstadoSuscriptor.Prueba;
        var suscripcionActiva = suscripcion is not null && suscripcion.FechaVencimiento >= DateTime.UtcNow;

        return new LicenciaDto
        {
            Existe = true,
            EstaActiva = clienteActivo && suscripcionActiva,
            NombreComercial = suscriptor.NombreComercial,
            Plan = suscripcion?.Plan.Nombre,
            Vence = suscripcion?.FechaVencimiento,
            UltimoPago = ultimoPago?.Fecha,
            EstadoCliente = suscriptor.Estado,
            EstadoSuscripcion = suscripcion?.Estado
        };
    }
}