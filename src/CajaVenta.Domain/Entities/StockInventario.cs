namespace CajaVenta.Domain.Entities;

public class StockInventario
{
    public Guid ProductoId { get; set; }
    public Guid SucursalId { get; set; }
    public decimal StockActual { get; set; }

    public Producto Producto { get; set; } = null!;
    public Sucursal Sucursal { get; set; } = null!;
}