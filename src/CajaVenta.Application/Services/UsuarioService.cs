using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Application.Security;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ISucursalRepository _sucursalRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository, ISucursalRepository sucursalRepository)
    {
        _usuarioRepository = usuarioRepository;
        _sucursalRepository = sucursalRepository;
    }

    public async Task<Result<List<UsuarioDto>>> ObtenerTodosAsync()
    {
        var usuarios = await _usuarioRepository.ObtenerTodosAsync();
        return Result<List<UsuarioDto>>.Success(usuarios.Select(MapearADto).ToList());
    }

    public async Task<Result<UsuarioDto>> ObtenerPorIdAsync(Guid id)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        return usuario is null
            ? Result<UsuarioDto>.Failure("Usuario no encontrado")
            : Result<UsuarioDto>.Success(MapearADto(usuario));
    }

    public async Task<Result<UsuarioDto>> CrearAsync(CrearUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<UsuarioDto>.Failure("El nombre es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
            return Result<UsuarioDto>.Failure("El nombre de usuario es obligatorio");
        if (string.IsNullOrWhiteSpace(dto.Contrasena) || dto.Contrasena.Length < 6)
            return Result<UsuarioDto>.Failure("La contraseña debe tener al menos 6 caracteres");

        var existente = await _usuarioRepository.ObtenerPorNombreUsuarioAsync(dto.NombreUsuario.Trim());
        if (existente is not null)
            return Result<UsuarioDto>.Failure($"Ya existe el usuario '{dto.NombreUsuario.Trim()}'");

        if (dto.SucursalId.HasValue)
        {
            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(dto.SucursalId.Value);
            if (sucursal is null)
                return Result<UsuarioDto>.Failure("Sucursal no encontrada");
        }

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            NombreUsuario = dto.NombreUsuario.Trim(),
            ContrasenaHash = PasswordHasher.Hash(dto.Contrasena),
            Email = dto.Email?.Trim(),
            Rol = dto.Rol ?? "Cajero",
            SucursalId = dto.SucursalId,
            Activo = true
        };

        await _usuarioRepository.CrearAsync(usuario);
        return Result<UsuarioDto>.Success(MapearADto(usuario));
    }

    public async Task<Result<UsuarioDto>> ActualizarAsync(Guid id, ActualizarUsuarioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return Result<UsuarioDto>.Failure("El nombre es obligatorio");

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (usuario is null)
            return Result<UsuarioDto>.Failure("Usuario no encontrado");

        if (dto.SucursalId.HasValue && dto.SucursalId.Value != usuario.SucursalId)
        {
            var sucursal = await _sucursalRepository.ObtenerPorIdAsync(dto.SucursalId.Value);
            if (sucursal is null)
                return Result<UsuarioDto>.Failure("Sucursal no encontrada");
        }

        usuario.Nombre = dto.Nombre.Trim();
        usuario.Email = dto.Email?.Trim();
        usuario.Rol = dto.Rol ?? "Cajero";
        usuario.SucursalId = dto.SucursalId;

        await _usuarioRepository.ActualizarAsync(usuario);
        return Result<UsuarioDto>.Success(MapearADto(usuario));
    }

    public async Task<Result<bool>> ActivarAsync(Guid id, bool activo)
    {
        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (usuario is null)
            return Result<bool>.Failure("Usuario no encontrado");

        usuario.Activo = activo;
        await _usuarioRepository.ActualizarAsync(usuario);
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ReiniciarContrasenaAsync(Guid id, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6)
            return Result<bool>.Failure("La contraseña debe tener al menos 6 caracteres");

        var usuario = await _usuarioRepository.ObtenerPorIdAsync(id);
        if (usuario is null)
            return Result<bool>.Failure("Usuario no encontrado");

        usuario.ContrasenaHash = PasswordHasher.Hash(contrasena);
        await _usuarioRepository.ActualizarAsync(usuario);
        return Result<bool>.Success(true);
    }

    private static UsuarioDto MapearADto(Usuario u) => new()
    {
        Id = u.Id,
        Nombre = u.Nombre,
        NombreUsuario = u.NombreUsuario,
        Email = u.Email,
        Rol = u.Rol ?? "Cajero",
        SucursalId = u.SucursalId,
        SucursalNombre = u.Sucursal?.Nombre,
        Activo = u.Activo,
        UltimoAcceso = u.UltimoAcceso
    };
}