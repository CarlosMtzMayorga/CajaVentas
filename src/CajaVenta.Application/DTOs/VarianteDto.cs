namespace CajaVenta.Application.DTOs;

public record VarianteDto
{
    public Guid Id { get; init; }
    public string? Talla { get; init; }
    public string? Color { get; init; }
    public string? Presentacion { get; init; }
    public string? SKU { get; init; }
    public decimal PrecioAdicional { get; init; }
    public decimal StockActual { get; init; }
}
