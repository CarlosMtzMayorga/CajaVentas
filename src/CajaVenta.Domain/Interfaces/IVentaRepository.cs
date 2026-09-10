using CajaVenta.Domain.Entities;

namespace CajaVenta.Domain.Interfaces;

public interface IVentaRepository
{
    Task<Venta?> ObtenerPorIdAsync(Guid id);
    Task<Venta?> ObtenerPorNumeroTicketAsync(string numeroTicket);
    Task<IEnumerable<Venta>> ObtenerPorTurnoAsync(Guid turnoCajaId);
    Task<Venta> CrearAsync(Venta venta);
    Task ActualizarAsync(Venta venta);
    Task<IEnumerable<Venta>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null);
}
