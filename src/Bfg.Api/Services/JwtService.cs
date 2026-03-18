using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Bfg.Api.Configuration;
using Bfg.Core.Common;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bfg.Api.Services;

/// <summary>
/// Issues and validates JWT tokens compatible with SimpleJWT (user_id claim).
/// </summary>
public class JwtService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _key;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        var key = string.IsNullOrWhiteSpace(_options.SecretKey)
            ? "bfg-dev-fallback-secret-key-32-chars!"
            : _options.SecretKey;
        if (key.Length < 32) key = key.PadRight(32, '!');
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    }

    public (string Access, string Refresh) GeneratePair(User user)
    {
        var access = GenerateAccessToken(user);
        var refresh = GenerateRefreshToken(user);
        return (access, refresh);
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("user_id", user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email ?? "")
        };
        return WriteToken(claims, _options.AccessTokenLifetimeMinutes, "access");
    }

    public string GenerateRefreshToken(User user)
    {
        var claims = new List<Claim>
        {
            new("user_id", user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        return WriteToken(claims, _options.RefreshTokenLifetimeDays * 24 * 60, "refresh");
    }

    private string WriteToken(IEnumerable<Claim> claims, int lifetimeMinutes, string tokenType)
    {
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);
        var exp = DateTime.UtcNow.AddMinutes(lifetimeMinutes);
        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims.Append(new Claim("token_type", tokenType)),
            expires: exp,
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal? ValidateAccessToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            var type = principal.FindFirst("token_type")?.Value;
            return type == "access" ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    public int? GetUserIdFromRefreshToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out _);
            if (principal.FindFirst("token_type")?.Value != "refresh")
                return null;
            var userId = principal.FindFirst("user_id")?.Value;
            return int.TryParse(userId, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}
