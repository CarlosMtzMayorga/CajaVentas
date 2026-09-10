using CajaVenta.Domain.Enums;

namespace CajaVenta.Application.DTOs;

public record ProductoDto
{
    public Guid Id { get; init; }
    public string CodigoBarras { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal Costo { get; init; }
    public decimal PrecioVenta { get; init; }
    public TipoImpuesto TipoImpuesto { get; init; }
    public decimal StockMinimo { get; init; }
    public bool PermiteDecimales { get; init; }
    public string? Categoria { get; init; }
    public string? UnidadMedida { get; init; }
    public decimal Margen { get; init; }
}

public record ProductoBuscarDto
{
    public string CodigoBarras { get; init; } = string.Empty;
    public string? Nombre { get; init; }
    public decimal? PrecioVenta { get; init; }
    public TipoImpuesto TipoImpuesto { get; init; }
    public bool PermiteDecimales { get; init; }
    public List<VarianteDto> Variantes { get; init; } = new();
}

public record CrearProductoDto
{
    public string CodigoBarras { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string? Descripcion { get; init; }
    public decimal Costo { get; init; }
    public decimal PrecioVenta { get; init; }
    public TipoImpuesto TipoImpuesto { get; init; }
    public decimal StockMinimo { get; init; }
    public bool PermiteDecimales { get; init; }
    public string? Categoria { get; init; }
    public string? UnidadMedida { get; init; }
}

public record ActualizarProductoDto
{
    public string? Nombre { get; init; }
    public string? Descripcion { get; init; }
    public decimal? Costo { get; init; }
    public decimal? PrecioVenta { get; init; }
    public TipoImpuesto? TipoImpuesto { get; init; }
    public decimal? StockMinimo { get; init; }
    public bool? PermiteDecimales { get; init; }
    public string? Categoria { get; init; }
    public string? UnidadMedida { get; init; }
}
