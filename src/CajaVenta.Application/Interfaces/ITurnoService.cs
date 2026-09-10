using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface ITurnoService
{
    Task<Result<TurnoCajaDto>> AbrirTurnoAsync(AbrirTurnoDto dto);
    Task<Result<TurnoCajaDto>> CerrarTurnoAsync(Guid turnoId, CerrarTurnoDto dto);
    Task<Result<TurnoCajaDto>> ObtenerTurnoAbiertoAsync(Guid usuarioId);
    Task<Result<CorteXDto>> GenerarCorteXAsync(Guid turnoId);
    Task<Result<List<TurnoCajaDto>>> ObtenerHistorialAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null);
}
