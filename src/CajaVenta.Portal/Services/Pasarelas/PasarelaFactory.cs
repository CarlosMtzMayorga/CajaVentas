using System.Net.Http;

namespace CajaVenta.Portal.Services.Pasarelas;

public class PasarelaFactory
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<PasarelaFactory> _logger;

    public PasarelaFactory(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        IHttpContextAccessor httpContextAccessor,
        ILogger<PasarelaFactory> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public IPasarelaPago Crear()
    {
        var seccion = _configuration.GetSection("Pasarela");
        var proveedor = seccion["Proveedor"] ?? "Demo";

        switch (proveedor.ToLowerInvariant())
        {
            case "stripe":
                var stripeKey = seccion["ClaveSecreta"] ?? seccion["Stripe:ClaveSecreta"];
                if (string.IsNullOrWhiteSpace(stripeKey))
                {
                    _logger.LogWarning("Stripe seleccionado pero sin clave; se usará modo Demo.");
                    return new DemoPasarela(_httpContextAccessor);
                }
                return new StripePasarela(_httpClientFactory.CreateClient("Pasarela"), stripeKey);

            case "mercadopago":
            case "mercado_pago":
                var mpKey = seccion["TokenAcceso"] ?? seccion["MercadoPago:TokenAcceso"];
                if (string.IsNullOrWhiteSpace(mpKey))
                {
                    _logger.LogWarning("MercadoPago seleccionado pero sin token; se usará modo Demo.");
                    return new DemoPasarela(_httpContextAccessor);
                }
                return new MercadoPagoPasarela(_httpClientFactory.CreateClient("Pasarela"), mpKey);

            default:
                return new DemoPasarela(_httpContextAccessor);
        }
    }
}