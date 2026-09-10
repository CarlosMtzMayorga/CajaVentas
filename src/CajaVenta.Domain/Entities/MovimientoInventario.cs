using CajaVenta.Domain.Common;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Domain.Entities;

public class MovimientoInventario : BaseEntity
{
    public Guid ProductoId { get; set; }
    public Guid SucursalId { get; set; }
    public TipoMovimientoInventario Tipo { get; set; }
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    public DateTime Fecha { get; set; }

    public Producto Producto { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
}
