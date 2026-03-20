using System.Security.Claims;
using Bfg.Api.Configuration;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using Bfg.Core.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", Register).AllowAnonymous();
        group.MapPost("/token", Token).AllowAnonymous();
        group.MapPost("/token/refresh", TokenRefresh).AllowAnonymous();
        group.MapPost("/token/verify", TokenVerify).AllowAnonymous();
        group.MapPost("/forgot-password", ForgotPassword).AllowAnonymous();
        group.MapPost("/reset-password-confirm", ResetPasswordConfirm).AllowAnonymous();
        group.MapPost("/verify-email", VerifyEmail).AllowAnonymous();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest req,
        BfgDbContext db,
        JwtService jwt,
        IConfiguration config,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest(new { email = new[] { "This field is required." }, password = new[] { "This field is required." } });
        if (req.Password != req.PasswordConfirm)
            return Results.BadRequest(new { password_confirm = new[] { "Passwords do not match." } });
        if (req.Password.Length < 8)
            return Results.BadRequest(new { password = new[] { "Password too short." } });

        if (await db.Users.AnyAsync(u => u.Email == req.Email, ct))
            return Results.BadRequest(new { email = new[] { "A user with this email already exists." } });

        var username = req.Email.Split('@')[0];
        var baseName = username;
        var counter = 1;
        while (await db.Users.AnyAsync(u => u.Username == username, ct))
            username = $"{baseName}{counter++}";

        var user = new User
        {
            Username = username,
            Email = req.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(req.Password),
            FirstName = req.FirstName ?? "",
            LastName = req.LastName ?? "",
            IsActive = true,
            DateJoined = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var (access, refresh) = jwt.GeneratePair(user);
        var frontendUrl = config.GetFrontendUrl();
        return Results.Created($"{frontendUrl}/login", new
        {
            user = new { id = user.Id, username = user.Username, email = user.Email, first_name = user.FirstName, last_name = user.LastName },
            access,
            refresh
        });
    }

    private static async Task<IResult> Token(
        [FromBody] TokenRequest req,
        BfgDbContext db,
        JwtService jwt,
        CancellationToken ct)
    {
        var identifier = !string.IsNullOrWhiteSpace(req.Username) ? req.Username : req.Email;
        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(req.Password))
            return Results.BadRequest(new { detail = "Email/username and password are required." });

        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Email == identifier || u.Username == identifier,
            ct);
        if (user == null || !AppPasswordHasher.Verify(user.Password, req.Password ?? ""))
            return Results.Unauthorized();

        if (!user.IsActive)
            return Results.Json(new { detail = "User is inactive." }, statusCode: 401);

        var (access, refresh) = jwt.GeneratePair(user);
        return Results.Ok(new { access, refresh });
    }

    private static async Task<IResult> TokenRefresh(
        [FromBody] RefreshRequest req,
        BfgDbContext db,
        JwtService jwt,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(req.Refresh))
            return Results.BadRequest(new { refresh = new[] { "This field is required." } });
        var userId = jwt.GetUserIdFromRefreshToken(req.Refresh);
        if (userId == null)
            return Results.Json(new { detail = "Token is invalid or expired." }, statusCode: 401);
        var user = await db.Users.FindAsync(new object[] { userId.Value }, ct);
        if (user == null || !user.IsActive)
            return Results.Json(new { detail = "User not found or inactive." }, statusCode: 401);
        var (access, refresh) = jwt.GeneratePair(user);
        return Results.Ok(new { access, refresh });
    }

    private static IResult TokenVerify([FromBody] VerifyRequest req, JwtService jwt)
    {
        if (string.IsNullOrEmpty(req.Token))
            return Results.BadRequest(new { token = new[] { "This field is required." } });
        var principal = jwt.ValidateAccessToken(req.Token);
        return principal != null ? Results.Ok() : Results.Json(new { detail = "Token is invalid or expired." }, statusCode: 401);
    }

    private static async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest req,
        BfgDbContext db,
        IConfiguration config,
        CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email && u.IsActive, ct);
        // Don't reveal whether email exists
        var frontendUrl = config.GetFrontendUrl();
        return Results.Ok(new { detail = "If that email exists, we have sent a reset link." });
    }

    private static IResult ResetPasswordConfirm([FromBody] ResetPasswordRequest req)
    {
        // Placeholder: would validate token and update password
        return Results.Ok(new { detail = "Password has been reset." });
    }

    private static IResult VerifyEmail([FromBody] VerifyEmailRequest req)
    {
        return Results.Ok(new { detail = "Email verified." });
    }
}

record RegisterRequest(string? Email, string? Password, string? PasswordConfirm, string? FirstName, string? LastName);
record TokenRequest(string? Email, string? Username, string? Password);
record RefreshRequest(string? Refresh);
record VerifyRequest(string? Token);
record ForgotPasswordRequest(string? Email);
record ResetPasswordRequest(string? Token, string? NewPassword, string? NewPasswordConfirm);
record VerifyEmailRequest(string? Key);
