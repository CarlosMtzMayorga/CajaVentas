using CajaVenta.Domain.Common;

namespace CajaVenta.Domain.Entities;

public class VariantesProducto : BaseEntity
{
    public Guid ProductoId { get; set; }
    public string? Talla { get; set; }
    public string? Color { get; set; }
    public string? Presentacion { get; set; }
    public string? SKU { get; set; }
    public decimal PrecioAdicional { get; set; }
    public decimal StockActual { get; set; }

    public Producto Producto { get; set; } = null!;
}
