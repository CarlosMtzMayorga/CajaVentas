namespace CajaVenta.Application.DTOs;

public record UsuarioDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string NombreUsuario { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Rol { get; init; } = "Cajero";
    public Guid? SucursalId { get; init; }
    public string? SucursalNombre { get; init; }
    public bool Activo { get; init; }
    public DateTime? UltimoAcceso { get; init; }
}

public record CrearUsuarioDto
{
    public string Nombre { get; init; } = string.Empty;
    public string NombreUsuario { get; init; } = string.Empty;
    public string Contrasena { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Rol { get; init; } = "Cajero";
    public Guid? SucursalId { get; init; }
}

public record ActualizarUsuarioDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string Rol { get; init; } = "Cajero";
    public Guid? SucursalId { get; init; }
}

public record ReiniciarContrasenaDto
{
    public string Contrasena { get; init; } = string.Empty;
}