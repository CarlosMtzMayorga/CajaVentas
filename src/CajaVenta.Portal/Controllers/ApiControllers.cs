using CajaVenta.Portal.Enums;
using CajaVenta.Portal.Services;
using CajaVenta.Portal.Services.Pasarelas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Portal.Controllers;

[AllowAnonymous]
public class LicenciaController : ControllerBase
{
    private readonly ILicenciaService _licencias;

    public LicenciaController(ILicenciaService licencias) => _licencias = licencias;

    [HttpGet("api/licencia/{urlAcceso}")]
    public async Task<IActionResult> ObtenerLicencia(string urlAcceso)
    {
        var licencia = await _licencias.ObtenerLicenciaAsync(urlAcceso.Trim().ToLowerInvariant());
        if (licencia is null)
            return NotFound(new { Existe = false });
        return Ok(licencia);
    }
}

[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly ISuscripcionService _suscripciones;
    private readonly IPagoService _pagos;
    private readonly IPasarelaPago _pasarela;

    public WebhooksController(ISuscripcionService suscripciones, IPagoService pagos, IPasarelaPago pasarela)
    {
        _suscripciones = suscripciones;
        _pagos = pagos;
        _pasarela = pasarela;
    }

    [HttpPost("api/pagos/webhook/{proveedor}")]
    public async Task<IActionResult> RecibirWebhook(string proveedor, [FromBody] object payload)
    {
        var contenido = payload.ToString() ?? string.Empty;

        var referenciaExterna = await _pasarela.ObtenerReferenciaConfirmadaAsync(proveedor, contenido, null);

        if (string.IsNullOrWhiteSpace(referenciaExterna) || !Guid.TryParse(referenciaExterna, out var suscripcionId))
            return Ok(new { procesado = false });

        var suscripcion = await _suscripciones.ObtenerPorIdAsync(suscripcionId);
        if (suscripcion is null)
            return Ok(new { procesado = false });

        await _pagos.RegistrarAsync(
            suscripcion.SuscriptorId, suscripcion.Id, suscripcion.PrecioMensual,
            MetodoPagoPasarela.Tarjeta, "Pago confirmado vía webhook");

        return Ok(new { procesado = true });
    }
}