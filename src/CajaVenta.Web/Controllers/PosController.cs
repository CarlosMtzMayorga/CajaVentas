using System.Globalization;
using System.Security.Claims;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Enums;
using CajaVenta.Web.Models;
using CajaVenta.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CajaVenta.Web.Controllers;

[Authorize]
public class PosController : Controller
{
    private readonly IVentaService _ventaService;
    private readonly IProductoService _productoService;
    private readonly ITurnoService _turnoService;
    private readonly IClienteService _clienteService;
    private readonly IInventarioService _inventarioService;

    public PosController(
        IVentaService ventaService,
        IProductoService productoService,
        ITurnoService turnoService,
        IClienteService clienteService,
        IInventarioService inventarioService)
    {
        _ventaService = ventaService;
        _productoService = productoService;
        _turnoService = turnoService;
        _clienteService = clienteService;
        _inventarioService = inventarioService;
    }

    private Guid UsuarioId
        => Guid.Parse(User.FindFirstValue("UsuarioId")!);

    public async Task<IActionResult> Index()
    {
        var carrito = CarritoSesion.Obtener(HttpContext.Session);
        var turno = await _turnoService.ObtenerTurnoAbiertoAsync(UsuarioId);

        var modelo = new PosIndexModel
        {
            Carrito = carrito,
            Subtotal = Redondear(carrito.Sum(c => c.SubtotalLinea)),
            Impuestos = Redondear(carrito.Sum(c => c.ImpuestoLinea)),
            Total = Redondear(carrito.Sum(c => c.TotalLinea)),
            TurnoAbierto = turno.IsSuccess,
            Mensaje = TempData["Mensaje"] as string,
            Error = TempData["Error"] as string
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            TempData["Error"] = "Escribe o escanea un código";
            return RedirectToAction(nameof(Index));
        }

        var carrito = CarritoSesion.Obtener(HttpContext.Session);
        var (codigoLimpio, cantidad) = PosParser.ParseCodigoYCantidad(codigo.Trim());

        if (cantidad <= 0)
        {
            TempData["Error"] = "Código o cantidad no válida";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _productoService.ObtenerPorCodigoBarrasAsync(codigoLimpio);
        if (resultado.IsFailure)
        {
            var busqueda = await _productoService.BuscarAsync(codigoLimpio);
            if (busqueda.IsSuccess && busqueda.Value!.Count > 0)
            {
                await AgregarAlCarrito(carrito, busqueda.Value[0], cantidad);
                CarritoSesion.Guardar(HttpContext.Session, carrito);
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = resultado.Error ?? "Producto no encontrado";
            return RedirectToAction(nameof(Index));
        }

        await AgregarAlCarrito(carrito, resultado.Value!, cantidad);
        CarritoSesion.Guardar(HttpContext.Session, carrito);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Quitar(Guid productoId)
    {
        var carrito = CarritoSesion.Obtener(HttpContext.Session);
        carrito.RemoveAll(c => c.ProductoId == productoId);
        if (carrito.Count == 0)
            CarritoSesion.Vaciar(HttpContext.Session);
        else
            CarritoSesion.Guardar(HttpContext.Session, carrito);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Vaciar()
    {
        CarritoSesion.Vaciar(HttpContext.Session);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IrACobro()
    {
        var carrito = CarritoSesion.Obtener(HttpContext.Session);
        if (carrito.Count == 0)
        {
            TempData["Error"] = "El carrito está vacío";
            return RedirectToAction(nameof(Index));
        }

        var turno = await _turnoService.ObtenerTurnoAbiertoAsync(UsuarioId);
        if (turno.IsFailure)
        {
            TempData["Error"] = "No hay un turno abierto";
            return RedirectToAction(nameof(Index));
        }

        TempData["TurnoCajaId"] = turno.Value!.Id;
        return RedirectToAction(nameof(Cobro));
    }

    public async Task<IActionResult> Cobro()
    {
        var carrito = CarritoSesion.Obtener(HttpContext.Session);
        if (carrito.Count == 0)
            return RedirectToAction(nameof(Index));

        var clientes = await _clienteService.ObtenerTodosAsync(soloActivos: true);

        var modelo = new CobroModel
        {
            Carrito = carrito,
            Subtotal = Redondear(carrito.Sum(c => c.SubtotalLinea)),
            Impuestos = Redondear(carrito.Sum(c => c.ImpuestoLinea)),
            Total = Redondear(carrito.Sum(c => c.TotalLinea)),
            MontoRecibido = Redondear(carrito.Sum(c => c.TotalLinea)).ToString("F2", CultureInfo.InvariantCulture),
            Clientes = clientes.IsSuccess ? clientes.Value! : new List<ClienteDto>(),
            Error = TempData["Error"] as string
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCobro(CobroModel modelo)
    {
        var carrito = CarritoSesion.Obtener(HttpContext.Session);
        if (carrito.Count == 0)
            return RedirectToAction(nameof(Index));

        var total = Redondear(carrito.Sum(c => c.TotalLinea));

        if (!decimal.TryParse(modelo.MontoRecibido, NumberStyles.Number, CultureInfo.InvariantCulture, out var monto) || monto < 0)
        {
            TempData["Error"] = "Monto no válido";
            return RedirectToAction(nameof(Cobro));
        }

        if (modelo.MetodoPago == MetodoPago.Efectivo && monto < total)
        {
            TempData["Error"] = "El monto recibido es insuficiente";
            return RedirectToAction(nameof(Cobro));
        }

        var turno = await _turnoService.ObtenerTurnoAbiertoAsync(UsuarioId);
        if (turno.IsFailure)
        {
            TempData["Error"] = "No hay un turno de caja abierto";
            return RedirectToAction(nameof(Cobro));
        }

        var dto = new CrearVentaDto
        {
            TurnoCajaId = turno.Value!.Id,
            ClienteId = modelo.ClienteId,
            MetodoPago = modelo.MetodoPago,
            MontoRecibido = monto,
            Detalles = carrito.Select(c => new CrearDetalleVentaDto
            {
                ProductoId = c.ProductoId,
                Cantidad = c.Cantidad
            }).ToList()
        };

        var resultado = await _ventaService.CrearVentaAsync(dto);
        if (resultado.IsFailure)
        {
            TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Cobro));
        }

        CarritoSesion.Vaciar(HttpContext.Session);
        TempData["VentaExitosa"] = "Venta registrada correctamente";
        return RedirectToAction(nameof(Ticket), new { id = resultado.Value!.Id });
    }

    public async Task<IActionResult> Ticket(Guid id)
    {
        var resultado = await _ventaService.ObtenerPorIdAsync(id);
        if (resultado.IsFailure)
            return RedirectToAction(nameof(Index));

        ViewData["MensajeExito"] = TempData["VentaExitosa"] as string;
        return View(resultado.Value!);
    }

    private async Task AgregarAlCarrito(List<CarritoItem> carrito, ProductoDto producto, decimal cantidad)
    {
        var sucursalId = SucursalContext.ObtenerSucursalClaim(User);
        if (sucursalId is null)
        {
            var turno = await _turnoService.ObtenerTurnoAbiertoAsync(UsuarioId);
            if (turno.IsSuccess)
                sucursalId = turno.Value!.SucursalId;
        }

        decimal? stockDisponible = null;
        if (sucursalId.HasValue)
            stockDisponible = (await _inventarioService.ObtenerStockAsync(sucursalId.Value, producto.Id)).Value;

        if (stockDisponible is not null && stockDisponible <= 0)
        {
            TempData["Error"] = $"Sin stock: {producto.Nombre}";
            return;
        }

        var existente = carrito.FirstOrDefault(c => c.ProductoId == producto.Id);
        if (existente is not null)
        {
            var nuevaCantidad = existente.Cantidad + cantidad;
            if (stockDisponible is not null && nuevaCantidad > stockDisponible)
            {
                TempData["Error"] = $"Sin stock para {producto.Nombre}: {stockDisponible:N0} disponibles ({existente.Cantidad:N0} en carrito)";
                return;
            }

            existente.Cantidad = nuevaCantidad;
            existente.StockActual = stockDisponible ?? 0;
            TempData["Mensaje"] = $"Agregado: {producto.Nombre} x{nuevaCantidad:N0}";
            return;
        }

        if (stockDisponible is not null && cantidad > stockDisponible)
        {
            TempData["Error"] = $"Sin stock suficiente: {producto.Nombre} ({stockDisponible:N0} disponibles)";
            return;
        }

        carrito.Add(new CarritoItem
        {
            ProductoId = producto.Id,
            Nombre = producto.Nombre,
            CodigoBarras = producto.CodigoBarras,
            Cantidad = cantidad,
            PrecioUnitario = producto.PrecioVenta,
            StockActual = stockDisponible ?? 0,
            TipoImpuesto = producto.TipoImpuesto
        });

        TempData["Mensaje"] = $"Agregado: {producto.Nombre} x{cantidad:N0}";
    }

    private static decimal Redondear(decimal valor)
        => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}