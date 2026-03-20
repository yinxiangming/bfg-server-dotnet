namespace Bfg.Api.Infrastructure;

/// <summary>
/// Isolates anonymous storefront carts per browser/session (header or cookie).
/// </summary>
public static class StorefrontCartSession
{
    public const string HeaderName = "X-Bfg-Cart-Session";
    public const string CookieName = "bfg_cart_session";

    /// <summary>
    /// Prefer client header, then cookie; otherwise issue a new id and Set-Cookie.
    /// </summary>
    public static string Resolve(HttpContext ctx)
    {
        if (ctx.Request.Headers.TryGetValue(HeaderName, out var hv))
        {
            var h = hv.FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(h))
                return h;
        }

        if (ctx.Request.Cookies.TryGetValue(CookieName, out var cv) && !string.IsNullOrWhiteSpace(cv))
            return cv.Trim();

        var key = Guid.NewGuid().ToString("N");
        ctx.Response.Cookies.Append(
            CookieName,
            key,
            new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(30),
                Path = "/",
                SameSite = SameSiteMode.Lax
            });
        return key;
    }
}
