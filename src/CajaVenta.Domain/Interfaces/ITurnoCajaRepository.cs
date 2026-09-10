using CajaVenta.Domain.Entities;

namespace CajaVenta.Domain.Interfaces;

public interface ITurnoCajaRepository
{
    Task<TurnoCaja?> ObtenerAbiertoAsync(Guid usuarioId);
    Task<TurnoCaja?> ObtenerAbiertoEnCajaAsync(Guid cajaId);
    Task<TurnoCaja?> ObtenerPorIdAsync(Guid id);
    Task<TurnoCaja> CrearAsync(TurnoCaja turno);
    Task ActualizarAsync(TurnoCaja turno);
    Task<IEnumerable<TurnoCaja>> ObtenerHistorialAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null);
}
