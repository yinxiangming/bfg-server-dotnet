using Bfg.Api.Configuration;
using Bfg.Api.Services;
using Bfg.Core.Common;
using Microsoft.Extensions.Options;

namespace Bfg.Api.Tests;

public class JwtServiceTests
{
    private static JwtService CreateService()
    {
        var opts = Options.Create(new JwtOptions
        {
            SecretKey = "unit-test-secret-key-at-least-32-chars!!",
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenLifetimeMinutes = 60,
            RefreshTokenLifetimeDays = 7
        });
        return new JwtService(opts);
    }

    [Fact]
    public void ValidateAccessToken_round_trips_access_token()
    {
        var jwt = CreateService();
        var user = new User
        {
            Id = 42,
            Username = "u1",
            Email = "a@b.c"
        };
        var access = jwt.GenerateAccessToken(user);
        var principal = jwt.ValidateAccessToken(access);
        Assert.NotNull(principal);
        Assert.Equal("42", principal!.FindFirst("user_id")?.Value);
    }

    [Fact]
    public void GetUserIdFromRefreshToken_round_trips_refresh_token()
    {
        var jwt = CreateService();
        var user = new User { Id = 99, Username = "u2", Email = "" };
        var refresh = jwt.GenerateRefreshToken(user);
        var id = jwt.GetUserIdFromRefreshToken(refresh);
        Assert.Equal(99, id);
    }

    [Fact]
    public void GetUserIdFromRefreshToken_returns_null_for_access_token()
    {
        var jwt = CreateService();
        var user = new User { Id = 1, Username = "x", Email = "" };
        var access = jwt.GenerateAccessToken(user);
        Assert.Null(jwt.GetUserIdFromRefreshToken(access));
    }

    [Fact]
    public void ValidateAccessToken_returns_null_for_refresh_token()
    {
        var jwt = CreateService();
        var user = new User { Id = 1, Username = "x", Email = "" };
        var refresh = jwt.GenerateRefreshToken(user);
        Assert.Null(jwt.ValidateAccessToken(refresh));
    }
}
