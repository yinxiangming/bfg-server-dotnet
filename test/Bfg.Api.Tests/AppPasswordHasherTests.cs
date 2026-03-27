using Bfg.Api.Services;

namespace Bfg.Api.Tests;

public class AppPasswordHasherTests
{
    /// <summary>
    /// Vector from Rfc2898DeriveBytes (SHA256, 1000 iterations, salt "testsalt", password "testpass").
    /// </summary>
    private const string DjangoPbkdf2Vector =
        "pbkdf2_sha256$1000$testsalt$gRe9MYWZlIzf6JDnXv80Sfznc7+qFsFZailGKZG9ygg=";

    [Fact]
    public void Verify_django_pbkdf2_sha256_accepts_correct_password()
    {
        Assert.True(AppPasswordHasher.Verify(DjangoPbkdf2Vector, "testpass"));
    }

    [Fact]
    public void Verify_django_pbkdf2_sha256_rejects_wrong_password()
    {
        Assert.False(AppPasswordHasher.Verify(DjangoPbkdf2Vector, "wrong"));
    }

    [Fact]
    public void Verify_bcrypt_accepts_matching_password()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("mypassword", workFactor: 4);
        Assert.True(AppPasswordHasher.Verify(hash, "mypassword"));
    }

    [Fact]
    public void Verify_bcrypt_rejects_mismatched_password()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("mypassword", workFactor: 4);
        Assert.False(AppPasswordHasher.Verify(hash, "other"));
    }

    [Fact]
    public void Verify_empty_inputs_returns_false()
    {
        Assert.False(AppPasswordHasher.Verify("", "x"));
        Assert.False(AppPasswordHasher.Verify("hash", null!));
    }
}
