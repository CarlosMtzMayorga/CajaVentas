using System.Text.Json;
using CajaVenta.Web.Models;

namespace CajaVenta.Web.Services;

public static class CarritoSesion
{
    private const string Clave = "CarritoPOS";

    public static List<CarritoItem> Obtener(ISession session)
    {
        var json = session.GetString(Clave);
        if (string.IsNullOrEmpty(json))
            return new List<CarritoItem>();
        return JsonSerializer.Deserialize<List<CarritoItem>>(json)?.ToList() ?? new List<CarritoItem>();
    }

    public static void Guardar(ISession session, List<CarritoItem> carrito)
        => session.SetString(Clave, JsonSerializer.Serialize(carrito));

    public static void Vaciar(ISession session)
        => session.Remove(Clave);
}