using System.Globalization;

namespace CajaVenta.Web.Services;

public static class PosParser
{
    public static (string codigo, decimal cantidad) ParseCodigoYCantidad(string input)
    {
        var texto = input.Replace(" ", "").Trim();

        var idx = -1;
        foreach (var sep in new[] { 'x', 'X', '*' })
        {
            var i = texto.IndexOf(sep);
            if (i >= 0 && (idx < 0 || i < idx))
                idx = i;
        }

        if (idx > 0 && idx < texto.Length - 1)
        {
            var qtyText = texto[(idx + 1)..];
            if (decimal.TryParse(qtyText, NumberStyles.Number, CultureInfo.InvariantCulture, out var cantidad) && cantidad > 0)
                return (texto[..idx].Trim(), cantidad);
        }

        return (texto, 1m);
    }
}