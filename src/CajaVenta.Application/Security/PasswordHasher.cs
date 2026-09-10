using System.Security.Cryptography;
using System.Text;

namespace CajaVenta.Application.Security;

public static class PasswordHasher
{
    private const string Salt = "CajaVenta_Salt_2024";

    public static string Hash(string contrasena)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(contrasena + Salt));
        return Convert.ToBase64String(bytes);
    }

    public static bool Verificar(string contrasena, string hashAlmacenado)
        => Hash(contrasena) == hashAlmacenado;
}