namespace CajaVenta.Application.DTOs;

public record ClienteDto
{
    public Guid Id { get; init; }
    public string NombreRazonSocial { get; init; } = string.Empty;
    public string? NombreComercial { get; init; }
    public string RFC { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefono { get; init; }
    public string? CodigoPostal { get; init; }
    public string? RegimenFiscal { get; init; }
    public string? UsoCFDI { get; init; }
    public bool TieneConstancia { get; init; }
    public string? ConstanciaRuta { get; init; }
    public string? ConstanciaOriginalNombre { get; init; }
    public string? Notas { get; init; }
    public bool Activo { get; init; }
    public DateTime FechaCreacion { get; init; }
}

public record CrearClienteDto
{
    public string NombreRazonSocial { get; init; } = string.Empty;
    public string? NombreComercial { get; init; }
    public string RFC { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Telefono { get; init; }
    public string? CodigoPostal { get; init; }
    public string? RegimenFiscal { get; init; }
    public string? UsoCFDI { get; init; }
    public string? ConstanciaRuta { get; init; }
    public string? ConstanciaOriginalNombre { get; init; }
    public string? Notas { get; init; }
}

public record ActualizarClienteDto
{
    public string NombreRazonSocial { get; init; } = string.Empty;
    public string? NombreComercial { get; init; }
    public string? Email { get; init; }
    public string? Telefono { get; init; }
    public string? CodigoPostal { get; init; }
    public string? RegimenFiscal { get; init; }
    public string? UsoCFDI { get; init; }
    public string? ConstanciaRuta { get; init; }
    public string? ConstanciaOriginalNombre { get; init; }
    public string? Notas { get; init; }
}