using CajaVenta.Application.DTOs;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Web.Models;

public class ReportesModel
{
    public Guid SucursalId { get; set; }
    public string SucursalNombre { get; set; } = string.Empty;
    public List<SucursalDto> Sucursales { get; set; } = new();
    public Guid CajaId { get; set; }
    public string CajaNombre { get; set; } = string.Empty;
    public List<CajaDto> Cajas { get; set; } = new();
    public DateTime Desde { get; set; }
    public DateTime Hasta { get; set; }
    public List<VentaDto> Ventas { get; set; } = new();
    public List<CorteZDto> CortesZ { get; set; } = new();
    public List<ResumenDetalle> TopProductos { get; set; } = new();
    public List<ResumenMetodo> PorMetodo { get; set; } = new();
    public List<StockProductoDto> StockBajo { get; set; } = new();

    public decimal TotalVentas => Ventas.Where(v => v.Estado == EstadoVenta.Completada).Sum(v => v.Total);
    public decimal TotalImpuestos => Ventas.Where(v => v.Estado == EstadoVenta.Completada).Sum(v => v.Impuestos);
    public decimal TotalSubtotal => Ventas.Where(v => v.Estado == EstadoVenta.Completada).Sum(v => v.Subtotal);
    public int CantidadTickets => Ventas.Count(v => v.Estado == EstadoVenta.Completada);
    public decimal PromedioTicket => CantidadTickets > 0 ? TotalVentas / CantidadTickets : 0;
    public int TicketsCancelados => Ventas.Count(v => v.Estado == EstadoVenta.Cancelada);
}

public class ResumenDetalle
{
    public string Producto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal Importe { get; set; }
}

public class ResumenMetodo
{
    public MetodoPago Metodo { get; set; }
    public decimal Importe { get; set; }
    public int Tickets { get; set; }
}