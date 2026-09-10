using System.Net.Http.Headers;
using System.Text.Json;

namespace CajaVenta.Portal.Services.Pasarelas;

public class StripePasarela : IPasarelaPago
{
    private readonly HttpClient _http;
    private readonly string _claveSecreta;

    public StripePasarela(HttpClient http, string claveSecreta)
    {
        _http = http;
        _claveSecreta = claveSecreta;
    }

    public string Nombre => "Stripe";

    public async Task<ResultadoCobro> CrearCobroAsync(DatosCobro datos)
    {
        var query = new Dictionary<string, string>
        {
            ["mode"] = "payment",
            ["amount"] = ((long)(datos.Monto * 100)).ToString(),
            ["currency"] = "mxn",
            ["description"] = datos.Concepto,
            ["success_url"] = datos.UrlRetorno,
            ["cancel_url"] = datos.UrlCancelacion,
            ["metadata[suscripcionId]"] = datos.SuscripcionId.ToString(),
            ["metadata[suscriptorId]"] = datos.SuscriptorId.ToString()
        };
        if (!string.IsNullOrWhiteSpace(datos.EmailCliente))
            query["customer_email"] = datos.EmailCliente;

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions")
        {
            Content = new FormUrlEncodedContent(query)
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _claveSecreta);

        using var respuesta = await _http.SendAsync(request);
        var cuerpo = await respuesta.Content.ReadAsStringAsync();
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException($"Stripe error {respuesta.StatusCode}: {cuerpo}");

        using var doc = JsonDocument.Parse(cuerpo);
        var id = doc.RootElement.GetProperty("id").GetString()!;
        var url = doc.RootElement.GetProperty("url").GetString()!;
        return new ResultadoCobro(id, url);
    }

    public Task<string?> ObtenerReferenciaConfirmadaAsync(string proveedor, string contenido, string? tipo)
    {
        if (!string.Equals(proveedor, Nombre, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<string?>(null);

        try
        {
            using var doc = JsonDocument.Parse(contenido);
            var evento = doc.RootElement;
            var eventoTipo = evento.TryGetProperty("type", out var t) ? t.GetString() : null;
            if (eventoTipo != "checkout.session.completed")
                return Task.FromResult<string?>(null);

            var objeto = evento.GetProperty("data").GetProperty("object");
            return Task.FromResult<string?>(objeto.GetProperty("id").GetString());
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }
}