using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface IVentaService
{
    Task<Result<VentaDto>> CrearVentaAsync(CrearVentaDto dto);
    Task<Result<VentaDto>> ObtenerPorIdAsync(Guid id);
    Task<Result<List<VentaDto>>> ObtenerPorTurnoAsync(Guid turnoCajaId);
    Task<Result<bool>> CancelarVentaAsync(Guid ventaId, string motivo);
    Task<Result<List<VentaDto>>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null, Guid? cajaId = null);
}
