using System.Security.Cryptography;
using System.Text;

namespace Bfg.Api.Services;

/// <summary>
/// Verifies passwords from the shared Django DB (pbkdf2_sha256) and dotnet-registered users (bcrypt).
/// </summary>
public static class AppPasswordHasher
{
    public static bool Verify(string storedHash, string plainPassword)
    {
        if (string.IsNullOrEmpty(storedHash) || plainPassword == null)
            return false;

        if (storedHash.StartsWith("pbkdf2_sha256$", StringComparison.Ordinal))
            return VerifyDjangoPbkdf2Sha256(storedHash, plainPassword);

        // Bcrypt hashes from this API or Node ($2a$, $2b$, etc.)
        if (storedHash.StartsWith("$2", StringComparison.Ordinal))
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, storedHash);
        }
        catch
        {
            return false;
        }
    }

    private static bool VerifyDjangoPbkdf2Sha256(string encoded, string password)
    {
        var parts = encoded.Split('$', 4);
        if (parts.Length != 4 || parts[0] != "pbkdf2_sha256")
            return false;
        if (!int.TryParse(parts[1], out var iterations))
            return false;
        var salt = parts[2];
        byte[] expected;
        try
        {
            expected = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            Encoding.UTF8.GetBytes(salt),
            iterations,
            HashAlgorithmName.SHA256);
        var actual = pbkdf2.GetBytes(expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
