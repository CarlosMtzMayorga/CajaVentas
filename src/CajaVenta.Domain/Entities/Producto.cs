using CajaVenta.Domain.Common;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Domain.Entities;

public class Producto : AuditableEntity
{
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal Costo { get; set; }
    public decimal PrecioVenta { get; set; }
    public TipoImpuesto TipoImpuesto { get; set; } = TipoImpuesto.IVA16;
    public decimal StockMinimo { get; set; }
    public bool PermiteDecimales { get; set; }
    public string? Categoria { get; set; }
    public string? UnidadMedida { get; set; }

    public ICollection<VariantesProducto> Variantes { get; set; } = new List<VariantesProducto>();
    public ICollection<DetalleVenta> DetallesVenta { get; set; } = new List<DetalleVenta>();
    public ICollection<MovimientoInventario> MovimientosInventario { get; set; } = new List<MovimientoInventario>();
    public ICollection<StockInventario> StocksInventario { get; set; } = new List<StockInventario>();
}
