using CajaVenta.Application.DTOs;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Web.Models;

public class ProductosIndexModel
{
    public string? Busqueda { get; set; }
    public List<ProductoDto> Productos { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public class EditarProductoModel
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Costo { get; set; }
    public decimal PrecioVenta { get; set; }
    public TipoImpuesto TipoImpuesto { get; set; }
    public decimal StockMinimo { get; set; }
    public string? Categoria { get; set; }
    public string? UnidadMedida { get; set; }
}

public class MovimientoModel
{
    public Guid SucursalId { get; set; }
    public Guid ProductoId { get; set; }
    public TipoMovimientoInventario Tipo { get; set; } = TipoMovimientoInventario.Entrada;
    public decimal Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }
    public string? Referencia { get; set; }
    public string? Observaciones { get; set; }
    public List<SucursalDto> Sucursales { get; set; } = new();
    public List<StockProductoDto> Productos { get; set; } = new();
    public string? Error { get; set; }
}

public static class ImpuestoUI
{
    public static string Nombre(TipoImpuesto tipo)
        => tipo switch
        {
            TipoImpuesto.IVA16 => "IVA 16%",
            TipoImpuesto.IVA8Frontera => "IVA 8% (Frontera)",
            TipoImpuesto.Exento => "Exento",
            TipoImpuesto.IEPS => "IEPS",
            _ => tipo.ToString()
        };

    public static string Nombre(TipoMovimientoInventario tipo)
        => tipo switch
        {
            TipoMovimientoInventario.Entrada => "Entrada",
            TipoMovimientoInventario.Salida => "Salida",
            TipoMovimientoInventario.Venta => "Venta",
            TipoMovimientoInventario.Ajuste => "Ajuste",
            TipoMovimientoInventario.Devolucion => "Devolución",
            _ => tipo.ToString()
        };
}