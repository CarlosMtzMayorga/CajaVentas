using CajaVenta.Application.DTOs;

namespace CajaVenta.Web.Models;

public class UsuariosIndexModel
{
    public List<UsuarioDto> Usuarios { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public class CrearUsuarioModel
{
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Rol { get; set; } = "Cajero";
    public Guid? SucursalId { get; set; }
    public List<SucursalDto> Sucursales { get; set; } = new();
}

public class EditarUsuarioModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string Rol { get; set; } = "Cajero";
    public Guid? SucursalId { get; set; }
    public List<SucursalDto> Sucursales { get; set; } = new();
}