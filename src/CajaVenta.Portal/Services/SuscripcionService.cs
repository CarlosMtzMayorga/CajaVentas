using CajaVenta.Portal.Data;
using CajaVenta.Portal.Entities;
using CajaVenta.Portal.Enums;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Services;

public interface ISuscripcionService
{
    Task<List<Suscripcion>> ObtenerTodasAsync();
    Task<Suscripcion?> ObtenerPorIdAsync(Guid id);
    Task<List<Suscripcion>> ObtenerPorSuscriptorAsync(Guid suscriptorId);
    Task<ResultadoOperacion> CrearAsync(Guid suscriptorId, Guid planId, decimal precioMensual,
        DateTime fechaInicio, int meses, EstadoSuscripcion estado);
    Task<ResultadoOperacion> ActualizarAsync(Guid id, Guid planId, decimal precioMensual,
        DateTime fechaInicio, DateTime fechaVencimiento, EstadoSuscripcion estado);
    Task<ResultadoOperacion> CancelarAsync(Guid id);
    Task<ResultadoOperacion> MarcarVencidaAsync(Guid id);
    Task<ResultadoOperacion> ActivarAsync(Guid id);
    Task<List<Suscripcion>> ObtenerVencidasAsync();
}

public class SuscripcionService : ISuscripcionService
{
    private readonly PortalDbContext _context;

    public SuscripcionService(PortalDbContext context) => _context = context;

    public Task<List<Suscripcion>> ObtenerTodasAsync() =>
        _context.Suscripciones
            .Include(s => s.Suscriptor)
            .Include(s => s.Plan)
            .AsNoTracking()
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

    public Task<Suscripcion?> ObtenerPorIdAsync(Guid id) =>
        _context.Suscripciones
            .Include(s => s.Suscriptor)
            .Include(s => s.Plan)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Suscripcion>> ObtenerPorSuscriptorAsync(Guid suscriptorId) =>
        _context.Suscripciones
            .Include(s => s.Plan)
            .AsNoTracking()
            .Where(s => s.SuscriptorId == suscriptorId)
            .OrderByDescending(s => s.FechaCreacion)
            .ToListAsync();

    public async Task<ResultadoOperacion> CrearAsync(Guid suscriptorId, Guid planId, decimal precioMensual,
        DateTime fechaInicio, int meses, EstadoSuscripcion estado)
    {
        var suscriptor = await _context.Suscriptores.FindAsync(suscriptorId);
        if (suscriptor is null || suscriptor.Estado == EstadoSuscriptor.Cancelado)
            return ResultadoOperacion.Fallo("Cliente no válido para suscripción.");

        var plan = await _context.Planes.FindAsync(planId);
        if (plan is null)
            return ResultadoOperacion.Fallo("Plan no encontrado.");

        var dias = meses * 30;
        var suscripcion = new Suscripcion
        {
            Id = Guid.NewGuid(),
            SuscriptorId = suscriptorId,
            PlanId = planId,
            PrecioMensual = precioMensual,
            FechaInicio = fechaInicio,
            FechaVencimiento = fechaInicio.AddDays(dias),
            Estado = estado,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Suscripciones.Add(suscripcion);
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(suscripcion.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Guid id, Guid planId, decimal precioMensual,
        DateTime fechaInicio, DateTime fechaVencimiento, EstadoSuscripcion estado)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion is null)
            return ResultadoOperacion.Fallo("Suscripción no encontrada.");

        suscripcion.PlanId = planId;
        suscripcion.PrecioMensual = precioMensual;
        suscripcion.FechaInicio = fechaInicio;
        suscripcion.FechaVencimiento = fechaVencimiento;
        suscripcion.Estado = estado;

        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }

    public async Task<ResultadoOperacion> CancelarAsync(Guid id)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion is null)
            return ResultadoOperacion.Fallo("Suscripción no encontrada.");

        suscripcion.Estado = EstadoSuscripcion.Cancelada;
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }

    public async Task<ResultadoOperacion> MarcarVencidaAsync(Guid id)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion is null)
            return ResultadoOperacion.Fallo("Suscripción no encontrada.");

        suscripcion.Estado = EstadoSuscripcion.Vencida;
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }

    public async Task<ResultadoOperacion> ActivarAsync(Guid id)
    {
        var suscripcion = await _context.Suscripciones.FindAsync(id);
        if (suscripcion is null)
            return ResultadoOperacion.Fallo("Suscripción no encontrada.");

        suscripcion.Estado = EstadoSuscripcion.Activa;
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }

    public Task<List<Suscripcion>> ObtenerVencidasAsync() =>
        _context.Suscripciones
            .Include(s => s.Suscriptor)
            .Include(s => s.Plan)
            .AsNoTracking()
            .Where(s => s.Estado == EstadoSuscripcion.Activa && s.FechaVencimiento < DateTime.UtcNow)
            .ToListAsync();
}