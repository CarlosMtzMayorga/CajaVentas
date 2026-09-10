using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public class VentaService : IVentaService
{
    private readonly IVentaRepository _ventaRepository;
    private readonly IProductoRepository _productoRepository;
    private readonly ITurnoCajaRepository _turnoRepository;
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VentaService(
        IVentaRepository ventaRepository,
        IProductoRepository productoRepository,
        ITurnoCajaRepository turnoRepository,
        IInventarioRepository inventarioRepository,
        IClienteRepository clienteRepository,
        IUnitOfWork unitOfWork)
    {
        _ventaRepository = ventaRepository;
        _productoRepository = productoRepository;
        _turnoRepository = turnoRepository;
        _inventarioRepository = inventarioRepository;
        _clienteRepository = clienteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<VentaDto>> CrearVentaAsync(CrearVentaDto dto)
    {
        if (dto.Detalles.Count == 0)
            return Result<VentaDto>.Failure("La venta debe tener al menos un producto");

        var turno = await _turnoRepository.ObtenerPorIdAsync(dto.TurnoCajaId);
        if (turno is null || turno.Estado != EstadoTurno.Abierto)
            return Result<VentaDto>.Failure("El turno de caja no está abierto");

        Cliente? cliente = null;
        if (dto.ClienteId.HasValue)
        {
            cliente = await _clienteRepository.ObtenerPorIdAsync(dto.ClienteId.Value);
            if (cliente is null || !cliente.Activo)
                return Result<VentaDto>.Failure("El cliente seleccionado no es válido");
        }

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            var venta = new Venta
            {
                Id = Guid.NewGuid(),
                TurnoCajaId = dto.TurnoCajaId,
                ClienteId = cliente?.Id,
                Estado = EstadoVenta.Completada,
                MetodoPago = dto.MetodoPago,
                MontoRecibido = dto.MontoRecibido,
                Observaciones = dto.Observaciones,
                NumeroTicket = await GenerarNumeroTicketAsync()
            };

            decimal subtotalVenta = 0;
            decimal impuestosVenta = 0;

            foreach (var detalleDto in dto.Detalles)
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(detalleDto.ProductoId);
                if (producto is null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<VentaDto>.Failure($"Producto {detalleDto.ProductoId} no encontrado");
                }

                if (!producto.PermiteDecimales && detalleDto.Cantidad != Math.Floor(detalleDto.Cantidad))
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<VentaDto>.Failure(
                        $"El producto '{producto.Nombre}' no permite venta fraccionada");
                }

                var stock = await _inventarioRepository.ObtenerStockAsync(turno.SucursalId, producto.Id);
                var stockDisponible = stock?.StockActual ?? 0m;

                if (stockDisponible < detalleDto.Cantidad)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<VentaDto>.Failure(
                        $"Stock insuficiente para '{producto.Nombre}'. Disponible: {stockDisponible}");
                }

                decimal precioUnitario = producto.PrecioVenta;
                if (detalleDto.VarianteId.HasValue)
                {
                    var variante = producto.Variantes.FirstOrDefault(v => v.Id == detalleDto.VarianteId.Value);
                    if (variante is null)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return Result<VentaDto>.Failure("Variante no encontrada");
                    }
                    precioUnitario += variante.PrecioAdicional;
                }

                var tasaImpuesto = Calc.ObtenerTasaImpuesto(producto.TipoImpuesto);
                var subtotalDetalle = Calc.CalcularIVA(precioUnitario * detalleDto.Cantidad, 1m);
                var impuestoDetalle = Calc.CalcularIVA(subtotalDetalle, tasaImpuesto);
                var totalDetalle = subtotalDetalle + impuestoDetalle;

                var detalle = new DetalleVenta
                {
                    Id = Guid.NewGuid(),
                    VentaId = venta.Id,
                    ProductoId = producto.Id,
                    VarianteId = detalleDto.VarianteId,
                    Cantidad = detalleDto.Cantidad,
                    PrecioUnitario = precioUnitario,
                    Subtotal = subtotalDetalle,
                    Impuesto = impuestoDetalle,
                    Total = totalDetalle,
                    TipoImpuesto = producto.TipoImpuesto
                };

                venta.Detalles.Add(detalle);
                subtotalVenta += subtotalDetalle;
                impuestosVenta += impuestoDetalle;

                if (stock is null)
                {
                    stock = new StockInventario
                    {
                        ProductoId = producto.Id,
                        SucursalId = turno.SucursalId,
                        StockActual = stockDisponible
                    };
                    await _inventarioRepository.CrearStockAsync(stock);
                }

                stock.StockActual -= detalleDto.Cantidad;
                await _inventarioRepository.ActualizarStockAsync(stock);

                await _inventarioRepository.RegistrarMovimientoAsync(new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    SucursalId = turno.SucursalId,
                    Tipo = TipoMovimientoInventario.Venta,
                    Cantidad = -detalleDto.Cantidad,
                    CostoUnitario = producto.Costo,
                    Referencia = venta.NumeroTicket,
                    Fecha = DateTime.UtcNow
                });
            }

            venta.Subtotal = subtotalVenta;
            venta.Impuestos = impuestosVenta;
            venta.Total = subtotalVenta + impuestosVenta;
            venta.Cambio = dto.MetodoPago == MetodoPago.Efectivo
                ? Math.Max(0, dto.MontoRecibido - venta.Total)
                : 0;

            if (dto.MetodoPago == MetodoPago.Efectivo && dto.MontoRecibido < venta.Total)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return Result<VentaDto>.Failure("El monto recibido es menor al total");
            }

            turno.VentasEfectivo += dto.MetodoPago == MetodoPago.Efectivo ? venta.Total : 0;
            turno.TotalVentas += venta.Total;
            await _turnoRepository.ActualizarAsync(turno);

            await _ventaRepository.CrearAsync(venta);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return Result<VentaDto>.Success(MapearAVentaDto(venta));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<VentaDto>.Failure($"Error al procesar venta: {ex.Message}");
        }
    }

    public async Task<Result<VentaDto>> ObtenerPorIdAsync(Guid id)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(id);
        return venta is null
            ? Result<VentaDto>.Failure("Venta no encontrada")
            : Result<VentaDto>.Success(MapearAVentaDto(venta));
    }

    public async Task<Result<List<VentaDto>>> ObtenerPorTurnoAsync(Guid turnoCajaId)
    {
        var ventas = await _ventaRepository.ObtenerPorTurnoAsync(turnoCajaId);
        return Result<List<VentaDto>>.Success(ventas.Select(MapearAVentaDto).ToList());
    }

    public async Task<Result<bool>> CancelarVentaAsync(Guid ventaId, string motivo)
    {
        var venta = await _ventaRepository.ObtenerPorIdAsync(ventaId);
        if (venta is null)
            return Result<bool>.Failure("Venta no encontrada");

        if (venta.Estado == EstadoVenta.Cancelada)
            return Result<bool>.Failure("La venta ya está cancelada");

        var turno = await _turnoRepository.ObtenerPorIdAsync(venta.TurnoCajaId);

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            venta.Estado = EstadoVenta.Cancelada;
            venta.Observaciones = $"CANCELADA - {motivo}";

            foreach (var detalle in venta.Detalles)
            {
                var producto = await _productoRepository.ObtenerPorIdAsync(detalle.ProductoId);
                if (producto is null || turno is null)
                    continue;

                var stock = await _inventarioRepository.ObtenerStockAsync(turno.SucursalId, producto.Id);
                if (stock is null)
                {
                    stock = new StockInventario
                    {
                        ProductoId = producto.Id,
                        SucursalId = turno.SucursalId,
                        StockActual = 0
                    };
                    await _inventarioRepository.CrearStockAsync(stock);
                }

                stock.StockActual += detalle.Cantidad;
                await _inventarioRepository.ActualizarStockAsync(stock);

                await _inventarioRepository.RegistrarMovimientoAsync(new MovimientoInventario
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    SucursalId = turno.SucursalId,
                    Tipo = TipoMovimientoInventario.Devolucion,
                    Cantidad = detalle.Cantidad,
                    CostoUnitario = producto.Costo,
                    Referencia = $"CANCEL-{venta.NumeroTicket}",
                    Fecha = DateTime.UtcNow
                });
            }

            if (turno is not null)
            {
                if (venta.MetodoPago == MetodoPago.Efectivo)
                    turno.VentasEfectivo -= venta.Total;
                turno.TotalVentas -= venta.Total;
                await _turnoRepository.ActualizarAsync(turno);
            }

            await _ventaRepository.ActualizarAsync(venta);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result<bool>.Failure($"Error al cancelar venta: {ex.Message}");
        }
    }

    public async Task<Result<List<VentaDto>>> ObtenerPorRangoFechasAsync(DateTime inicio, DateTime fin, Guid? sucursalId = null)
    {
        var ventas = await _ventaRepository.ObtenerPorRangoFechasAsync(inicio, fin, sucursalId);
        return Result<List<VentaDto>>.Success(ventas.Select(MapearAVentaDto).ToList());
    }

    private async Task<string> GenerarNumeroTicketAsync()
    {
        var fecha = DateTime.UtcNow;
        var ventasDelDia = await _ventaRepository.ObtenerPorRangoFechasAsync(
            fecha.Date, fecha.Date.AddDays(1).AddTicks(-1));
        var consecutive = ventasDelDia.Count() + 1;
        return $"T{fecha:yyyyMMdd}-{consecutive:D5}";
    }

    private static VentaDto MapearAVentaDto(Venta v) => new()
    {
        Id = v.Id,
        NumeroTicket = v.NumeroTicket,
        FechaCreacion = v.FechaCreacion,
        Estado = v.Estado,
        Subtotal = v.Subtotal,
        Impuestos = v.Impuestos,
        Total = v.Total,
        MetodoPago = v.MetodoPago,
        MontoRecibido = v.MontoRecibido,
        Cambio = v.Cambio,
        ClienteId = v.ClienteId,
        ClienteNombre = v.Cliente?.NombreRazonSocial,
        ClienteRfc = v.Cliente?.RFC,
        Detalles = v.Detalles.Select(d => new DetalleVentaDto
        {
            Id = d.Id,
            ProductoNombre = d.Producto.Nombre,
            ProductoCodigoBarras = d.Producto.CodigoBarras,
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario,
            Subtotal = d.Subtotal,
            Impuesto = d.Impuesto,
            Total = d.Total
        }).ToList()
    };
}