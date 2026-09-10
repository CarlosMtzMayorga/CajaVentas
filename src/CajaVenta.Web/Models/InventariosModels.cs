using CajaVenta.Application.DTOs;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Web.Models;

public enum EstadoStock
{
    Ok,
    Bajo,
    SinExistencias
}

public class StockItem
{
    public Guid ProductoId { get; set; }
    public string CodigoBarras { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal Costo { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal Valor => StockActual * Costo;
    public EstadoStock Estado
        => StockActual <= 0 ? EstadoStock.SinExistencias
         : StockActual <= StockMinimo ? EstadoStock.Bajo
         : EstadoStock.Ok;
}

public class InventarioResumenModel
{
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = string.Empty;
    public List<SucursalDto> Sucursales { get; set; } = new();
    public List<StockItem> Stock { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
    public int TotalSkus => Stock.Count;
    public decimal UnidadesTotales => Stock.Sum(s => s.StockActual);
    public decimal ValorInventario => Stock.Sum(s => s.Valor);
    public int ConExistencias => Stock.Count(s => s.StockActual > 0);
    public int SinExistencias => Stock.Count(s => s.StockActual <= 0);
    public int PorDebajoMinimo => Stock.Count(s => s.StockActual > 0 && s.StockActual <= s.StockMinimo);
    public int StockSobrante => Stock.Count(s => s.Estado == EstadoStock.Ok);
}

public class InventarioMovimientosModel
{
    public Guid? SucursalId { get; set; }
    public List<SucursalDto> Sucursales { get; set; } = new();
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
    public TipoMovimientoInventario? Tipo { get; set; }
    public Guid? ProductoId { get; set; }
    public string? Termino { get; set; }
    public string? ProductoNombre { get; set; }
    public List<ProductoDto> Productos { get; set; } = new();
    public List<MovimientoInventarioDto> Movimientos { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public class HistorialModel
{
    public Guid ProductoId { get; set; }
    public string ProductoNombre { get; set; } = string.Empty;
    public List<MovimientoInventarioDto> Movimientos { get; set; } = new();
}