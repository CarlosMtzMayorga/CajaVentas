using CajaVenta.Domain.Common;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Domain.Entities;

public class DetalleVenta : BaseEntity
{
    public Guid VentaId { get; set; }
    public Guid ProductoId { get; set; }
    public Guid? VarianteId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public TipoImpuesto TipoImpuesto { get; set; }

    public Venta Venta { get; set; } = null!;
    public Producto Producto { get; set; } = null!;
    public VariantesProducto? Variante { get; set; }
}
