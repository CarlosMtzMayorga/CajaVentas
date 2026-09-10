using CajaVenta.Portal.Data;
using CajaVenta.Portal.Entities;
using Microsoft.EntityFrameworkCore;

namespace CajaVenta.Portal.Services;

public interface IPlanService
{
    Task<List<Plan>> ObtenerTodosAsync();
    Task<List<Plan>> ObtenerActivosAsync();
    Task<Plan?> ObtenerPorIdAsync(Guid id);
    Task<ResultadoOperacion> CrearAsync(string nombre, string? descripcion, decimal precioMensual, bool activo);
    Task<ResultadoOperacion> ActualizarAsync(Guid id, string nombre, string? descripcion, decimal precioMensual, bool activo);
    Task<ResultadoOperacion> EliminarAsync(Guid id);
}

public class PlanService : IPlanService
{
    private readonly PortalDbContext _context;

    public PlanService(PortalDbContext context) => _context = context;

    public async Task<List<Plan>> ObtenerTodosAsync()
    {
        var planes = await _context.Planes.AsNoTracking().ToListAsync();
        return planes.OrderBy(p => p.PrecioMensual).ToList();
    }

    public async Task<List<Plan>> ObtenerActivosAsync()
    {
        var planes = await _context.Planes.AsNoTracking().Where(p => p.Activo).ToListAsync();
        return planes.OrderBy(p => p.PrecioMensual).ToList();
    }

    public Task<Plan?> ObtenerPorIdAsync(Guid id) =>
        _context.Planes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<ResultadoOperacion> CrearAsync(string nombre, string? descripcion, decimal precioMensual, bool activo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return ResultadoOperacion.Fallo("El nombre del plan es obligatorio.");
        if (precioMensual < 0)
            return ResultadoOperacion.Fallo("El precio mensual no puede ser negativo.");

        var plan = new Plan
        {
            Id = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            Descripcion = descripcion,
            PrecioMensual = precioMensual,
            Activo = activo,
            FechaCreacion = DateTime.UtcNow
        };

        _context.Planes.Add(plan);
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(plan.Id);
    }

    public async Task<ResultadoOperacion> ActualizarAsync(Guid id, string nombre, string? descripcion, decimal precioMensual, bool activo)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan is null)
            return ResultadoOperacion.Fallo("Plan no encontrado.");
        if (string.IsNullOrWhiteSpace(nombre))
            return ResultadoOperacion.Fallo("El nombre del plan es obligatorio.");
        if (precioMensual < 0)
            return ResultadoOperacion.Fallo("El precio mensual no puede ser negativo.");

        plan.Nombre = nombre.Trim();
        plan.Descripcion = descripcion;
        plan.PrecioMensual = precioMensual;
        plan.Activo = activo;

        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito(id);
    }

    public async Task<ResultadoOperacion> EliminarAsync(Guid id)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan is null)
            return ResultadoOperacion.Fallo("Plan no encontrado.");

        if (await _context.Suscripciones.AnyAsync(s => s.PlanId == id))
            return ResultadoOperacion.Fallo("No se puede eliminar un plan con suscripciones asociadas.");

        _context.Planes.Remove(plan);
        await _context.SaveChangesAsync();
        return ResultadoOperacion.Exito();
    }
}