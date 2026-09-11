using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Application.Services;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Enums;
using CajaVenta.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CajaVenta.Application.Tests.Services;

public class VentaServiceTests
{
    private readonly Mock<IVentaRepository> _ventaRepoMock;
    private readonly Mock<IProductoRepository> _productoRepoMock;
    private readonly Mock<ITurnoCajaRepository> _turnoRepoMock;
    private readonly Mock<IInventarioRepository> _inventarioRepoMock;
    private readonly Mock<IClienteRepository> _clienteRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly VentaService _ventaService;

    public VentaServiceTests()
    {
        _ventaRepoMock = new Mock<IVentaRepository>();
        _productoRepoMock = new Mock<IProductoRepository>();
        _turnoRepoMock = new Mock<ITurnoCajaRepository>();
        _inventarioRepoMock = new Mock<IInventarioRepository>();
        _clienteRepoMock = new Mock<IClienteRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _ventaService = new VentaService(
            _ventaRepoMock.Object,
            _productoRepoMock.Object,
            _turnoRepoMock.Object,
            _inventarioRepoMock.Object,
            _clienteRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CrearVentaAsync_ConDetallesVacios_DeberiaFallar()
    {
        // Arrange
        var dto = new CrearVentaDto
        {
            TurnoCajaId = Guid.NewGuid(),
            Detalles = new List<CrearDetalleVentaDto>()
        };

        // Act
        var resultado = await _ventaService.CrearVentaAsync(dto);

        // Assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be("La venta debe tener al menos un producto");
    }

    [Fact]
    public async Task CrearVentaAsync_ConTurnoCerrado_DeberiaFallar()
    {
        // Arrange
        var turnoId = Guid.NewGuid();
        var dto = new CrearVentaDto
        {
            TurnoCajaId = turnoId,
            Detalles = new List<CrearDetalleVentaDto>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 1 }
            }
        };

        _turnoRepoMock.Setup(x => x.ObtenerPorIdAsync(turnoId))
            .ReturnsAsync(new TurnoCaja { Id = turnoId, Estado = EstadoTurno.Cerrado });

        // Act
        var resultado = await _ventaService.CrearVentaAsync(dto);

        // Assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be("El turno de caja no está abierto");
    }

    [Fact]
    public async Task CrearVentaAsync_ConClienteInactivo_DeberiaFallar()
    {
        // Arrange
        var turnoId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var dto = new CrearVentaDto
        {
            TurnoCajaId = turnoId,
            ClienteId = clienteId,
            Detalles = new List<CrearDetalleVentaDto>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 1 }
            }
        };

        _turnoRepoMock.Setup(x => x.ObtenerPorIdAsync(turnoId))
            .ReturnsAsync(new TurnoCaja { Id = turnoId, Estado = EstadoTurno.Abierto, SucursalId = Guid.NewGuid() });

        _clienteRepoMock.Setup(x => x.ObtenerPorIdAsync(clienteId))
            .ReturnsAsync(new Cliente { Id = clienteId, Activo = false });

        // Act
        var resultado = await _ventaService.CrearVentaAsync(dto);

        // Assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be("El cliente seleccionado no es válido");
    }

    [Fact]
    public async Task CrearVentaAsync_ConProductoNoEncontrado_DeberiaFallar()
    {
        // Arrange
        var turnoId = Guid.NewGuid();
        var productoId = Guid.NewGuid();
        var dto = new CrearVentaDto
        {
            TurnoCajaId = turnoId,
            Detalles = new List<CrearDetalleVentaDto>
            {
                new() { ProductoId = productoId, Cantidad = 1 }
            }
        };

        _turnoRepoMock.Setup(x => x.ObtenerPorIdAsync(turnoId))
            .ReturnsAsync(new TurnoCaja { Id = turnoId, Estado = EstadoTurno.Abierto, SucursalId = Guid.NewGuid() });

        _productoRepoMock.Setup(x => x.ObtenerPorIdAsync(productoId))
            .ReturnsAsync((Producto?)null);

        // Act
        var resultado = await _ventaService.CrearVentaAsync(dto);

        // Assert
        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Contain($"Producto {productoId} no encontrado");
    }
}