using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;

namespace CajaVenta.Application.Interfaces;

public interface IUsuarioService
{
    Task<Result<List<UsuarioDto>>> ObtenerTodosAsync();
    Task<Result<UsuarioDto>> ObtenerPorIdAsync(Guid id);
    Task<Result<UsuarioDto>> CrearAsync(CrearUsuarioDto dto);
    Task<Result<UsuarioDto>> ActualizarAsync(Guid id, ActualizarUsuarioDto dto);
    Task<Result<bool>> ActivarAsync(Guid id, bool activo);
    Task<Result<bool>> ReiniciarContrasenaAsync(Guid id, string contrasena);
}