using CajaVenta.Domain.Enums;

namespace CajaVenta.Application.DTOs;

public record MovimientoInventarioDto
{
    public Guid Id { get; init; }
    public string ProductoNombre { get; init; } = string.Empty;
    public string ProductoCodigoBarras { get; init; } = string.Empty;
    public Guid SucursalId { get; init; }
    public string SucursalNombre { get; init; } = string.Empty;
    public TipoMovimientoInventario Tipo { get; init; }
    public decimal Cantidad { get; init; }
    public decimal CostoUnitario { get; init; }
    public string? Referencia { get; init; }
    public string? Observaciones { get; init; }
    public DateTime Fecha { get; init; }
}

public record RegistrarMovimientoDto
{
    public Guid SucursalId { get; init; }
    public Guid ProductoId { get; init; }
    public TipoMovimientoInventario Tipo { get; init; }
    public decimal Cantidad { get; init; }
    public decimal CostoUnitario { get; init; }
    public string? Referencia { get; init; }
    public string? Observaciones { get; init; }
}

public record StockProductoDto
{
    public Guid ProductoId { get; init; }
    public string CodigoBarras { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public decimal StockActual { get; init; }
    public decimal StockMinimo { get; init; }
    public decimal Costo { get; init; }
    public decimal PrecioVenta { get; init; }
    public TipoImpuesto TipoImpuesto { get; init; }
}
