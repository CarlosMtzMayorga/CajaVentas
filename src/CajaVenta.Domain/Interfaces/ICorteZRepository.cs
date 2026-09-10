using CajaVenta.Domain.Entities;

namespace CajaVenta.Domain.Interfaces;

public interface ICorteZRepository
{
    Task<CorteZ?> ObtenerPorTurnoAsync(Guid turnoCajaId);
    Task<CorteZ?> ObtenerPorIdAsync(Guid id);
    Task<CorteZ> CrearAsync(CorteZ corte);
    Task<int> ObtenerSiguienteNumeroAsync(Guid cajaId);
    Task<IEnumerable<CorteZ>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null, Guid? cajaId = null);
}