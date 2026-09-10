namespace CajaVenta.Application.DTOs;

public record SucursalDto
{
    public Guid Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Direccion { get; init; }
    public string? Telefono { get; init; }
    public string? Notas { get; init; }
    public bool Activo { get; init; }
    public int CantidadCajas { get; init; }
    public int CantidadUsuarios { get; init; }
}

public record CajaDto
{
    public Guid Id { get; init; }
    public Guid SucursalId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Notas { get; init; }
    public bool Activo { get; init; }
    public bool TurnoAbierto { get; init; }
}

public record CrearSucursalDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? Direccion { get; init; }
    public string? Telefono { get; init; }
    public string? Notas { get; init; }
}

public record ActualizarSucursalDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? Direccion { get; init; }
    public string? Telefono { get; init; }
    public string? Notas { get; init; }
}

public record CrearCajaDto
{
    public Guid SucursalId { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string? Notas { get; init; }
}

public record ActualizarCajaDto
{
    public string Nombre { get; init; } = string.Empty;
    public string? Notas { get; init; }
}