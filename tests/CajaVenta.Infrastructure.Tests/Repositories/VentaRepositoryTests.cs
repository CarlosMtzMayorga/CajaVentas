using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Infrastructure.Persistence;
using CajaVenta.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CajaVenta.Infrastructure.Tests.Repositories;

public class VentaRepositoryTests : IDisposable
{
    private readonly CajaVentaDbContext _context;
    private readonly VentaRepository _repository;
    private readonly ServiceProvider _serviceProvider;

    public VentaRepositoryTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<CajaVentaDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<CajaVentaDbContext>();
        _context.Database.OpenConnection();
        _context.Database.EnsureCreated();
        
        _repository = new VentaRepository(_context);
    }

    private async Task<(TurnoCaja turno, Producto producto)> SetupBasicDataAsync()
    {
        var sucursal = new Sucursal
        {
            Id = Guid.NewGuid(),
            Nombre = "Sucursal Test",
        };
        _context.Sucursales.Add(sucursal);

        var caja = new Caja
        {
            Id = Guid.NewGuid(),
            SucursalId = sucursal.Id,
            Nombre = "Caja Test",
        };
        _context.Cajas.Add(caja);

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = "Usuario Test",
            NombreUsuario = "testuser",
            ContrasenaHash = "hash",
            Rol = "Cajero",
            SucursalId = sucursal.Id,
        };
        _context.Usuarios.Add(usuario);

        var turno = new TurnoCaja
        {
            Id = Guid.NewGuid(),
            CajaId = caja.Id,
            SucursalId = sucursal.Id,
            UsuarioId = usuario.Id,
            Estado = EstadoTurno.Abierto,
            FondoInicial = 100m,
            FechaApertura = DateTime.UtcNow
        };
        _context.TurnosCaja.Add(turno);
        
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            CodigoBarras = "123456789",
            Nombre = "Producto Test",
            PrecioVenta = 100m,
            Costo = 50m,
            TipoImpuesto = TipoImpuesto.IVA16,
            StockMinimo = 5,
            PermiteDecimales = false
        };
        _context.Productos.Add(producto);
        
        await _context.SaveChangesAsync();
        
        return (turno, producto);
    }

    [Fact]
    public async Task CrearAsync_DeberiaGuardarVenta()
    {
        // Arrange
        var (turno, producto) = await SetupBasicDataAsync();
        
        var venta = new Venta
        {
            Id = Guid.NewGuid(),
            TurnoCajaId = turno.Id,
            NumeroTicket = "T20240101-00001",
            Estado = EstadoVenta.Completada,
            Subtotal = 100m,
            Impuestos = 16m,
            Total = 116m,
            MetodoPago = MetodoPago.Efectivo,
            MontoRecibido = 120m,
            Cambio = 4m,
            Detalles = new List<DetalleVenta>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    Cantidad = 1,
                    PrecioUnitario = 100m,
                    Subtotal = 100m,
                    Impuesto = 16m,
                    Total = 116m,
                    TipoImpuesto = TipoImpuesto.IVA16
                }
            }
        };

        // Act
        var resultado = await _repository.CrearAsync(venta);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(venta.Id);
        resultado.NumeroTicket.Should().Be("T20240101-00001");
    }

    [Fact]
    public async Task ObtenerPorIdAsync_DeberiaRetornarVenta()
    {
        // Arrange
        var (turno, producto) = await SetupBasicDataAsync();
        
        var venta = new Venta
        {
            Id = Guid.NewGuid(),
            TurnoCajaId = turno.Id,
            NumeroTicket = "T20240101-00001",
            Estado = EstadoVenta.Completada,
            Subtotal = 100m,
            Impuestos = 16m,
            Total = 116m,
            MetodoPago = MetodoPago.Efectivo,
            MontoRecibido = 120m,
            Cambio = 4m,
            Detalles = new List<DetalleVenta>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductoId = producto.Id,
                    Cantidad = 1,
                    PrecioUnitario = 100m,
                    Subtotal = 100m,
                    Impuesto = 16m,
                    Total = 116m,
                    TipoImpuesto = TipoImpuesto.IVA16
                }
            }
        };
        _context.Ventas.Add(venta);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObtenerPorIdAsync(venta.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(venta.Id);
        resultado.NumeroTicket.Should().Be("T20240101-00001");
        resultado.Detalles.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObtenerPorTurnoAsync_DeberiaRetornarVentasDelTurno()
    {
        // Arrange
        var (turno, producto) = await SetupBasicDataAsync();
        
        var ventas = new List<Venta>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TurnoCajaId = turno.Id,
                NumeroTicket = "T20240101-00001",
                Estado = EstadoVenta.Completada,
                Subtotal = 100m,
                Impuestos = 16m,
                Total = 116m,
                MetodoPago = MetodoPago.Efectivo,
                MontoRecibido = 120m,
                Cambio = 4m,
                Detalles = new List<DetalleVenta>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = producto.Id,
                        Cantidad = 1,
                        PrecioUnitario = 100m,
                        Subtotal = 100m,
                        Impuesto = 16m,
                        Total = 116m,
                        TipoImpuesto = TipoImpuesto.IVA16
                    }
                }
            },
            new()
            {
                Id = Guid.NewGuid(),
                TurnoCajaId = turno.Id,
                NumeroTicket = "T20240101-00002",
                Estado = EstadoVenta.Completada,
                Subtotal = 50m,
                Impuestos = 8m,
                Total = 58m,
                MetodoPago = MetodoPago.TarjetaCredito,
                MontoRecibido = 58m,
                Cambio = 0m,
                Detalles = new List<DetalleVenta>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ProductoId = producto.Id,
                        Cantidad = 1,
                        PrecioUnitario = 50m,
                        Subtotal = 50m,
                        Impuesto = 8m,
                        Total = 58m,
                        TipoImpuesto = TipoImpuesto.IVA16
                    }
                }
            }
        };
        _context.Ventas.AddRange(ventas);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ObtenerPorTurnoAsync(turno.Id);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().OnlyContain(v => v.TurnoCajaId == turno.Id);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}