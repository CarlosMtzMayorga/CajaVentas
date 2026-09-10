using CajaVenta.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.ViewComponents;

public class LicenciaStatusViewComponent : ViewComponent
{
    private readonly ILicenciaClienteService _licencia;

    public LicenciaStatusViewComponent(ILicenciaClienteService licencia)
        => _licencia = licencia;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!_licencia.EstaConfigurada)
            return Content(string.Empty);

        var estado = await _licencia.ObtenerEstadoAsync();
        ViewData["LicenciaPlan"] = estado.Plan;
        ViewData["LicenciaVence"] = estado.Vence;
        ViewData["LicenciaActiva"] = estado.EstaActiva;
        return View();
    }
}