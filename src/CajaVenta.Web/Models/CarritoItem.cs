using CajaVenta.Application.Common;
using CajaVenta.Domain.Enums;

namespace CajaVenta.Web.Models;

public class CarritoItem
{
    public Guid ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string CodigoBarras { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal StockActual { get; set; }
    public TipoImpuesto TipoImpuesto { get; set; }

    public decimal SubtotalLinea
        => Math.Round(PrecioUnitario * Cantidad, 2, MidpointRounding.AwayFromZero);

    public decimal ImpuestoLinea
        => Math.Round(SubtotalLinea * Calc.ObtenerTasaImpuesto(TipoImpuesto), 4, MidpointRounding.AwayFromZero);

    public decimal TotalLinea
        => Math.Round(SubtotalLinea + ImpuestoLinea, 2, MidpointRounding.AwayFromZero);
}