using Microsoft.AspNetCore.Http;

namespace CajaVenta.Portal.Services.Pasarelas;

public class DemoPasarela : IPasarelaPago
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DemoPasarela(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string Nombre => "Demo";

    public Task<ResultadoCobro> CrearCobroAsync(DatosCobro datos)
    {
        var baseUrl = $"{_httpContextAccessor.HttpContext!.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
        var referencia = $"{datos.SuscripcionId:N}";
        var url = $"{baseUrl}/Pagos/SimularPago?referencia={referencia}&monto={datos.Monto:N2}&suscripcionId={datos.SuscripcionId}";
        return Task.FromResult(new ResultadoCobro(referencia, url));
    }

    public Task<string?> ObtenerReferenciaConfirmadaAsync(string proveedor, string contenido, string? tipo)
    {
        if (!string.Equals(proveedor, Nombre, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>(null);

        return Task.FromResult<string?>(contenido);
    }
}