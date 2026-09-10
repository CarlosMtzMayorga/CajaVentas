using CajaVenta.Application.DTOs;

namespace CajaVenta.Web.Models;

public class ClientesIndexModel
{
    public string? Busqueda { get; set; }
    public List<ClienteDto> Clientes { get; set; } = new();
    public string? Mensaje { get; set; }
    public string? Error { get; set; }
}

public class ClienteFormModel
{
    public Guid Id { get; set; }
    public string NombreRazonSocial { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string? NombreComercial { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? CodigoPostal { get; set; }
    public string? RegimenFiscal { get; set; }
    public string? UsoCFDI { get; set; }
    public string? Notas { get; set; }
    public IFormFile? Constancia { get; set; }
    public bool TieneConstancia { get; set; }
    public string? ConstanciaOriginalNombre { get; set; }
}

public static class FiscalUI
{
    public static readonly (string Codigo, string Texto)[] UsosCFDI =
    {
        ("G01", "Adquisición de mercancías"),
        ("G03", "Gastos en general"),
        ("I01", "Construcciones"),
        ("I02", "Mobiliario y equipo de oficina"),
        ("I03", "Transporte"),
        ("I04", "Equipo de cómputo y accesorios"),
        ("I05", "Muebles, equipo e intangibles"),
        ("I06", "Otra maquinaria y equipo"),
        ("I07", "Equipo de transporte"),
        ("I08", "Otras opciones no mencionadas"),
        ("D01", "Honorarios médicos"),
        ("D02", "Gastos médicos por incapacidad"),
        ("D03", "Gastos funerarios"),
        ("D04", "Donativos"),
        ("D05", "Intereses reales de crédito hipotecario"),
        ("D06", "Aportaciones voluntarias al SAR"),
        ("D07", "Prima por seguros de gastos médicos"),
        ("D08", "Gastos de transporte escolar"),
        ("D09", "Depósitos en cuentas de ahorro"),
        ("D10", "Pagos por servicios educativos"),
        ("P01", "Por definir")
    };

    public static readonly (string Codigo, string Texto)[] RegimenesFiscales =
    {
        ("601", "General de Ley Personas Morales"),
        ("603", "Personas Morales con Fines no Lucrativos"),
        ("605", "Sueldos y Salarios e Ingresos asimilados a salarios"),
        ("606", "Arrendamiento"),
        ("608", "Demás ingresos"),
        ("609", "Consolidación"),
        ("610", "Residentes en el Extranjero sin Establecimiento Permanente"),
        ("611", "Ingresos por Dividendos (socios y accionistas)"),
        ("612", "Personas Físicas con Actividades Empresariales y Profesionales"),
        ("614", "Ingresos por intereses"),
        ("615", "Régimen de los ingresos por obtención de premios"),
        ("616", "Sin obligaciones fiscales"),
        ("621", "Incorporación Fiscal"),
        ("625", "Régimen de las Actividades Empresariales con Ingresos a través de Plataformas Tecnológicas"),
        ("626", "Régimen Simplificado de Confianza")
    };
}