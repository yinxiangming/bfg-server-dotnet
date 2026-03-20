using System.Security.Claims;

namespace Bfg.Api.Auth;

/// <summary>
/// Resolves authenticated user id from JWT (sub / NameIdentifier).
/// </summary>
public static class AuthUser
{
    public static bool TryGetUserId(HttpContext ctx, out int userId)
    {
        var v = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? ctx.User.FindFirst("sub")?.Value;
        return int.TryParse(v, out userId);
    }
}
