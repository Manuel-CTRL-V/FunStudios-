using System.Security.Cryptography;

namespace FunStudiosWebApp.Security;

/// <summary>
/// Utilidad para hashear y verificar contraseñas con PBKDF2 (Rfc2898DeriveBytes).
/// El formato almacenado es: {iteraciones}.{saltBase64}.{hashBase64}
/// No se guardan contraseñas en texto plano.
/// </summary>
public static class PasswordHasher
{
    private const int Iteraciones = 100_000;
    private const int TamanoSalta = 16;
    private const int TamanoHash = 32;

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanoSalta);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verificar(string password, string hashAlmacenado)
    {
        var partes = hashAlmacenado.Split('.', 3);
        if (partes.Length != 3) return false;

        var iteraciones = int.Parse(partes[0]);
        var salt = Convert.FromBase64String(partes[1]);
        var hashEsperado = Convert.FromBase64String(partes[2]);

        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(password, salt, iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

        return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
    }
}
