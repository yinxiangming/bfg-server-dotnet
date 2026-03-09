namespace Bfg.Api.Configuration;

/// <summary>
/// JWT settings for SimpleJWT-compatible tokens (env: JWT__SECRET_KEY, JWT__ISSUER, etc.).
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; set; } = "";
    public string Issuer { get; set; } = "bfg-api";
    public string Audience { get; set; } = "bfg-api";
    public int AccessTokenLifetimeMinutes { get; set; } = 60;
    public int RefreshTokenLifetimeDays { get; set; } = 7;
}
