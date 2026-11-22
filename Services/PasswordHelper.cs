using System.Security.Cryptography;
using System.Text;

namespace PetClinicLab3.Services;

public static class PasswordHelper
{
    private const int SaltSize   = 16;
    private const int HashSize   = 32;
    private const int Iterations = 600_000;

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt,
            Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash)) return false;
        string[] parts = storedHash.Split(':');
        if (parts.Length != 3) return false;
        if (!int.TryParse(parts[0], out int iterations) || iterations <= 0 || iterations > 2_000_000) return false;
        byte[] salt, storedBytes;
        try
        {
            salt        = Convert.FromBase64String(parts[1]);
            storedBytes = Convert.FromBase64String(parts[2]);
        }
        catch (FormatException) { return false; }
        if (salt.Length == 0 || storedBytes.Length == 0) return false;
        byte[] computed = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt,
            iterations, HashAlgorithmName.SHA256, storedBytes.Length);
        return CryptographicOperations.FixedTimeEquals(computed, storedBytes);
    }
}
