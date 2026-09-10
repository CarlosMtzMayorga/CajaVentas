using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface ICorteZService
{
    Task<Result<CorteZDto>> GenerarCorteZAsync(Guid turnoCajaId);
    Task<Result<CorteZDto>> ObtenerPorTurnoAsync(Guid turnoCajaId);
    Task<Result<CorteZDto>> ObtenerPorIdAsync(Guid id);
    Task<Result<List<CorteZDto>>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null, Guid? cajaId = null);
}