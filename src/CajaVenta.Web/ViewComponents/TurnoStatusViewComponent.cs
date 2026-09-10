using System.Security.Claims;
using CajaVenta.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.ViewComponents;

public class TurnoStatusViewComponent : ViewComponent
{
    private readonly ITurnoService _turnoService;

    public TurnoStatusViewComponent(ITurnoService turnoService)
        => _turnoService = turnoService;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var usuarioId = UserClaimsPrincipal?.FindFirstValue("UsuarioId");
        if (string.IsNullOrWhiteSpace(usuarioId) || !Guid.TryParse(usuarioId, out var uid))
            return Content(string.Empty);

        var resultado = await _turnoService.ObtenerTurnoAbiertoAsync(uid);
        if (resultado.IsFailure)
            return Content(string.Empty);

        var turno = resultado.Value!;
        ViewData["TurnoFondo"] = turno.FondoInicial;
        ViewData["TurnoVentas"] = turno.TotalVentas;
        ViewData["TurnoCantidad"] = turno.CantidadVentas;
        return View();
    }
}