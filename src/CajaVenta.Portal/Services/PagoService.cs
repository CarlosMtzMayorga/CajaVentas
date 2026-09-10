using CajaVenta.Portal.Data;
using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Services;

public interface IPagoService
{
    Task<List<Pago>> ObtenerTodosAsync();
    Task<List<Pago>> ObtenerPorSuscriptorAsync(Guid suscriptorId);
    Task<Pago?> ObtenerPorIdAsync(Guid id);
    Task<ResultadoOperacion> RegistrarAsync(Guid suscriptorId, Guid suscripcionId, decimal monto,
        MetodoPagoPasarela metodo, string? notas);
    Task<decimal> ObtenerIngresosDelMesAsync();
    Task<decimal> ObtenerIngresosTotalesAsync();
}

public class PagoService : IPagoService
{
    private readonly PortalDbContext _context;

    public PagoService(PortalDbContext context) => _context = context;

    public Task<List<Pago>> ObtenerTodosAsync() =>
        _context.Pagos
            .Include(p => p.Suscriptor)
            .Include(p => p.Suscripcion)
            .AsNoTracking()
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

    public Task<List<Pago>> ObtenerPorSuscriptorAsync(Guid suscriptorId) =>
        _context.Pagos
            .Include(p => p.Suscripcion)
            .AsNoTracking()
            .Where(p => p.SuscriptorId == suscriptorId)
            .OrderByDescending(p => p.Fecha)
            .ToListAsync();

    public Task<Pago?> ObtenerPorIdAsync(Guid id) =>
        _context.Pagos
            .Include(p => p.Suscriptor)
            .Include(p => p.Suscripcion)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<ResultadoOperacion> RegistrarAsync(Guid suscriptorId, Guid suscripcionId, decimal monto,
        MetodoPagoPasarela metodo, string? notas)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(suscripcionId);
        if (suscripcion is null)
            return ResultadoOperacion.Fallo("Suscripción no encontrada.");

        if (monto <= 0)
            return ResultadoOperacion.Fallo("El monto debe ser mayor a cero.");

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            SuscriptorId = suscriptorId,
            SuscripcionId = suscripcionId,
            Monto = monto,
            Metodo = metodo,
            Estado = EstadoPago.Pagado,
            Notas = notas,
            Fecha = DateTime.UtcNow
        };

        _context.Pagos.Add(pago);
        await _context.SaveChangesAsync();

        if (suscripcion.Estado != EstadoSuscripcion.Activa)
            suscripcion.Estado = EstadoSuscripcion.Activa;

        if (suscripcion.FechaVencimiento < DateTime.UtcNow)
            suscripcion.FechaVencimiento = DateTime.UtcNow.AddMonths(1);
        else
            suscripcion.FechaVencimiento = suscripcion.FechaVencimiento.AddMonths(1);

        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(pago.Id);
    }

    public async Task<decimal> ObtenerIngresosDelMesAsync()
    {
        var inicioMes = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var pagos = await _context.Pagos.AsNoTracking()
            .Where(p => p.Estado == EstadoPago.Pagado && p.Fecha >= inicioMes)
            .ToListAsync();
        return pagos.Sum(p => p.Monto);
    }

    public async Task<decimal> ObtenerIngresosTotalesAsync()
    {
        var pagos = await _context.Pagos.AsNoTracking()
            .Where(p => p.Estado == EstadoPago.Pagado)
            .ToListAsync();
        return pagos.Sum(p => p.Monto);
    }
}