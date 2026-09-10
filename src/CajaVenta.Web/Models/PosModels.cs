using CajaVenta.Application.DTOs;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Web.Models;

public class PosIndexModel
{
    public List<CarritoItem> Carrito { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public bool TurnoAbierto { get; set; }
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
    public int CantidadArticulos => Carrito.Sum(c => (int)c.Cantidad);
}

public class CobroModel
{
    public List<CarritoItem> Carrito { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
    public string MontoRecibido { get; set; } = string.Empty;
    public Guid? ClienteId { get; set; }
    public List<ClienteDto> Clientes { get; set; } = new();
    public string? Error { get; set; }
}

public static class MetodosPagoUI
{
    public static readonly (MetodoPago Valor, string Texto)[] Lista =
    {
        (MetodoPago.Efectivo, "Efectivo"),
        (MetodoPago.TarjetaCredito, "Tarjeta Crédito"),
        (MetodoPago.TarjetaDebito, "Tarjeta Débito"),
        (MetodoPago.Transferencia, "Transferencia"),
        (MetodoPago.QR, "QR")
    };
}