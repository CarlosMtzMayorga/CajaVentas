using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace CajaVenta.Web.Services;

public class LicenciaClienteDto
{
    public bool Existe { get; set; }
    public bool EstaActiva { get; set; }
    public string? NombreComercial { get; set; }
    public string? Plan { get; set; }
    public DateTime? Vence { get; set; }
    public DateTime? UltimoPago { get; set; }
}

public interface ILicenciaClienteService
{
    bool EstaConfigurada { get; }
    Task<LicenciaClienteDto> ObtenerEstadoAsync();
    Task<LicenciaClienteDto> ObtenerEstadoDirectoAsync();
}

public class LicenciaClienteService : ILicenciaClienteService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(30);
    private const string CacheKey = "licencia_cliente";

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly string? _portalUrl;
    private readonly string? _identificador;

    public LicenciaClienteService(HttpClient http, IMemoryCache cache, IConfiguration configuration)
    {
        _http = http;
        _cache = cache;
        _portalUrl = configuration["Licencia:PortalUrl"];
        _identificador = configuration["Licencia:Identificador"];
    }

    public bool EstaConfigurada =>
        !string.IsNullOrWhiteSpace(_portalUrl) && !string.IsNullOrWhiteSpace(_identificador);

    public async Task<LicenciaClienteDto> ObtenerEstadoAsync()
    {
        if (!EstaConfigurada)
            return new LicenciaClienteDto { Existe = false, EstaActiva = true };

        if (_cache.TryGetValue(CacheKey, out LicenciaClienteDto? cacheado) && cacheado is not null)
            return cacheado;

        var resultado = await ConsultarPortalAsync();
        _cache.Set(CacheKey, resultado, Ttl);
        return resultado;
    }

    public async Task<LicenciaClienteDto> ObtenerEstadoDirectoAsync()
    {
        if (!EstaConfigurada)
            return new LicenciaClienteDto { Existe = false, EstaActiva = true };

        return await ConsultarPortalAsync();
    }

    private async Task<LicenciaClienteDto> ConsultarPortalAsync()
    {
        try
        {
            var url = $"{_portalUrl!.TrimEnd('/')}/api/licencia/{Uri.EscapeDataString(_identificador!.Trim().ToLowerInvariant())}";
            using var respuesta = await _http.GetAsync(url);
            if (!respuesta.IsSuccessStatusCode)
                return new LicenciaClienteDto { Existe = false, EstaActiva = false };

            var json = await respuesta.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<LicenciaClienteDto>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return dto ?? new LicenciaClienteDto { Existe = false, EstaActiva = false };
        }
        catch
        {
            return new LicenciaClienteDto { Existe = false, EstaActiva = false };
        }
    }
}