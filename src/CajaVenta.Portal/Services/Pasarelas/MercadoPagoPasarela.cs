using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CajaVenta.Portal.Services.Pasarelas;

public class MercadoPagoPasarela : IPasarelaPago
{
    private readonly HttpClient _http;
    private readonly string _tokenAcceso;

    public MercadoPagoPasarela(HttpClient http, string tokenAcceso)
    {
        _http = http;
        _tokenAcceso = tokenAcceso;
    }

    public string Nombre => "MercadoPago";

    public async Task<ResultadoCobro> CrearCobroAsync(DatosCobro datos)
    {
        var payload = new Dictionary<string, object>
        {
            ["items"] = new[]
            {
                new Dictionary<string, object>
                {
                    ["title"] = datos.Concepto,
                    ["quantity"] = 1,
                    ["unit_price"] = datos.Monto,
                    ["currency_id"] = "MXN"
                }
            },
            ["notification_url"] = datos.UrlRetorno,
            ["back_urls"] = new Dictionary<string, string>
            {
                ["success"] = datos.UrlRetorno,
                ["failure"] = datos.UrlCancelacion,
                ["pending"] = datos.UrlCancelacion
            },
            ["auto_return"] = "approved",
            ["external_reference"] = datos.SuscripcionId.ToString()
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.mercadopago.com/checkout/preferences")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);

        using var respuesta = await _http.SendAsync(request);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException($"MercadoPago error {respuesta.StatusCode}: {cuerpo}");

        using var doc = JsonDocument.Parse(cuerpo);
        var id = doc.RootElement.GetProperty("id").GetString()!;
        var url = doc.RootElement.GetProperty("init_point").GetString()!;
        return new ResultadoCobro(id, url);
    }

    public async Task<string?> ObtenerReferenciaConfirmadaAsync(string proveedor, string contenido, string? tipo)
    {
        if (!string.Equals(proveedor, Nombre, StringComparison.OrdinalIgnoreCase))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(contenido);
            var raiz = doc.RootElement;
            string externalRef;

            if (raiz.TryGetProperty("data", out var data) && data.TryGetProperty("id", out var tipoPago))
            {
                var idPago = data.GetProperty("id").GetString();
                var pagoConsulta = await ConsultarPagoAsync(idPago);
                if (!pagoConsulta.HasValue)
                    return null;

                var elementoPago = pagoConsulta.Value;
                var estado = elementoPago.TryGetProperty("status", out var st) ? st.GetString() : null;
                if (estado != "approved")
                    return null;

                externalRef = elementoPago.TryGetProperty("external_reference", out var er)
                    ? er.GetString() ?? string.Empty
                    : string.Empty;
            }
            else if (raiz.TryGetProperty("external_reference", out var erDirecto)
                     && raiz.TryGetProperty("status", out var estadoDirecto)
                     && estadoDirecto.GetString() == "approved")
            {
                externalRef = erDirecto.GetString() ?? string.Empty;
            }
            else
            {
                return null;
            }

            return string.IsNullOrEmpty(externalRef) ? null : externalRef;
        }
        catch
        {
            return null;
        }
    }

    private async Task<JsonElement?> ConsultarPagoAsync(string? idPago)
    {
        if (string.IsNullOrWhiteSpace(idPago))
            return null;

        var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.mercadopago.com/v1/payments/{idPago}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _tokenAcceso);

        using var respuesta = await _http.SendAsync(request);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            return null;

        using var doc = JsonDocument.Parse(cuerpo);
        return doc.RootElement.Clone();
    }
}