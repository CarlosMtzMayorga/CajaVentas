namespace CajaVenta.Portal.Services.Pasarelas;

public record DatosCobro(
    Guid SuscripcionId,
    Guid SuscriptorId,
    decimal Monto,
    string Concepto,
    string? EmailCliente,
    string UrlRetorno,
    string UrlCancelacion);

public record ResultadoCobro(string ReferenciaExterna, string UrlPago);

public interface IPasarelaPago
{
    string Nombre { get; }
    Task<ResultadoCobro> CrearCobroAsync(DatosCobro datos);
    Task<string?> ObtenerReferenciaConfirmadaAsync(string proveedor, string contenido, string? tipo);
}