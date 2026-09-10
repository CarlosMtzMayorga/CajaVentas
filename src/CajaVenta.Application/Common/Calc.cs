namespace CajaVenta.Application.Common;

public static class Calc
{
    private const int PrecisionBD = 4;
    private const int PrecisionPresentacion = 2;

    public static decimal RedondearPresentacion(decimal valor)
        => Math.Round(valor, PrecisionPresentacion, MidpointRounding.AwayFromZero);

    public static decimal CalcularIVA(decimal baseImpuesto, decimal tasa)
        => Math.Round(baseImpuesto * tasa, PrecisionBD, MidpointRounding.AwayFromZero);

    public static decimal ObtenerTasaImpuesto(Domain.Enums.TipoImpuesto tipo)
        => tipo switch
        {
            Domain.Enums.TipoImpuesto.IVA16 => 0.16m,
            Domain.Enums.TipoImpuesto.IVA8Frontera => 0.08m,
            Domain.Enums.TipoImpuesto.Exento => 0m,
            Domain.Enums.TipoImpuesto.IEPS => 0m,
            _ => 0.16m
        };
}
