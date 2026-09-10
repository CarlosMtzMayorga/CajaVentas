using System.Text.RegularExpressions;
using CajaVenta.Application.Common;
using CajaVenta.Application.DTOs;
using CajaVenta.Application.Interfaces;
using CajaVenta.Domain.Entities;
using CajaVenta.Domain.Interfaces;

namespace CajaVenta.Application.Services;

public partial class ClienteService : IClienteService
{
    private readonly IClienteRepository _clienteRepository;

    public ClienteService(IClienteRepository clienteRepository)
        => _clienteRepository = clienteRepository;

    public async Task<Result<ClienteDto>> ObtenerPorIdAsync(Guid id)
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(id);
        return cliente is null
            ? Result<ClienteDto>.Failure("Cliente no encontrado")
            : Result<ClienteDto>.Success(MapearADto(cliente));
    }

    public async Task<Result<List<ClienteDto>>> ObtenerTodosAsync(bool soloActivos = false)
    {
        var clientes = await _clienteRepository.ObtenerTodosAsync(soloActivos);
        return Result<List<ClienteDto>>.Success(
            clientes.OrderBy(c => c.NombreRazonSocial).Select(MapearADto).ToList());
    }

    public async Task<Result<List<ClienteDto>>> BuscarAsync(string termino)
    {
        var clientes = await _clienteRepository.BuscarAsync(termino.Trim());
        return Result<List<ClienteDto>>.Success(
            clientes.OrderBy(c => c.NombreRazonSocial).Select(MapearADto).ToList());
    }

    public async Task<Result<ClienteDto>> CrearAsync(CrearClienteDto dto)
    {
        var error = Validar(dto.NombreRazonSocial, dto.RFC);
        if (error is not null)
            return Result<ClienteDto>.Failure(error);

        var rfc = NormalizarRFC(dto.RFC);
        var existente = await _clienteRepository.ObtenerPorRFCAsync(rfc);
        if (existente is not null)
            return Result<ClienteDto>.Failure($"Ya existe un cliente con el RFC {rfc}");

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            NombreRazonSocial = dto.NombreRazonSocial.Trim(),
            NombreComercial = Limpiar(dto.NombreComercial),
            RFC = rfc,
            Email = Limpiar(dto.Email),
            Telefono = Limpiar(dto.Telefono),
            CodigoPostal = Limpiar(dto.CodigoPostal),
            RegimenFiscal = Limpiar(dto.RegimenFiscal),
            UsoCFDI = Limpiar(dto.UsoCFDI),
            ConstanciaRuta = dto.ConstanciaRuta,
            ConstanciaOriginalNombre = dto.ConstanciaOriginalNombre,
            Notas = Limpiar(dto.Notas),
            Activo = true
        };

        await _clienteRepository.CrearAsync(cliente);
        return Result<ClienteDto>.Success(MapearADto(cliente));
    }

    public async Task<Result<ClienteDto>> ActualizarAsync(Guid id, ActualizarClienteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreRazonSocial))
            return Result<ClienteDto>.Failure("La razón social es obligatoria");

        var cliente = await _clienteRepository.ObtenerPorIdAsync(id);
        if (cliente is null)
            return Result<ClienteDto>.Failure("Cliente no encontrado");

        cliente.NombreRazonSocial = dto.NombreRazonSocial.Trim();
        cliente.NombreComercial = Limpiar(dto.NombreComercial);
        cliente.Email = Limpiar(dto.Email);
        cliente.Telefono = Limpiar(dto.Telefono);
        cliente.CodigoPostal = Limpiar(dto.CodigoPostal);
        cliente.RegimenFiscal = Limpiar(dto.RegimenFiscal);
        cliente.UsoCFDI = Limpiar(dto.UsoCFDI);
        cliente.Notas = Limpiar(dto.Notas);

        if (!string.IsNullOrWhiteSpace(dto.ConstanciaRuta))
        {
            cliente.ConstanciaRuta = dto.ConstanciaRuta;
            cliente.ConstanciaOriginalNombre = dto.ConstanciaOriginalNombre;
        }

        await _clienteRepository.ActualizarAsync(cliente);
        return Result<ClienteDto>.Success(MapearADto(cliente));
    }

    public async Task<Result<bool>> ActivarAsync(Guid id, bool activo)
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(id);
        if (cliente is null)
            return Result<bool>.Failure("Cliente no encontrado");

        cliente.Activo = activo;
        await _clienteRepository.ActualizarAsync(cliente);
        return Result<bool>.Success(true);
    }

    private static string? Validar(string nombre, string rfc)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return "La razón social es obligatoria";

        if (string.IsNullOrWhiteSpace(rfc))
            return "El RFC es obligatorio para poder facturar";

        return RfcRegex().IsMatch(NormalizarRFC(rfc))
            ? null
            : "El RFC no tiene un formato válido (ej. AAA010101AAA)";
    }

    private static string NormalizarRFC(string rfc)
        => rfc.Trim().ToUpperInvariant();

    private static string? Limpiar(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static ClienteDto MapearADto(Cliente c) => new()
    {
        Id = c.Id,
        NombreRazonSocial = c.NombreRazonSocial,
        NombreComercial = c.NombreComercial,
        RFC = c.RFC,
        Email = c.Email,
        Telefono = c.Telefono,
        CodigoPostal = c.CodigoPostal,
        RegimenFiscal = c.RegimenFiscal,
        UsoCFDI = c.UsoCFDI,
        TieneConstancia = !string.IsNullOrWhiteSpace(c.ConstanciaRuta),
        ConstanciaRuta = c.ConstanciaRuta,
        ConstanciaOriginalNombre = c.ConstanciaOriginalNombre,
        Notas = c.Notas,
        Activo = c.Activo,
        FechaCreacion = c.FechaCreacion
    };

    [GeneratedRegex(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$")]
    private static partial Regex RfcRegex();
}