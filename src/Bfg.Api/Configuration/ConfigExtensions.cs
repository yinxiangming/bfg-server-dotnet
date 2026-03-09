using Microsoft.Extensions.Configuration;

namespace Bfg.Api.Configuration;

/// <summary>
/// Centralized config access from env. Use these instead of reading IConfiguration keys elsewhere.
/// Env vars: DATABASE_URL, FRONTEND_URL, SITE_NAME; JWT__SECRET_KEY, JWT__ISSUER, etc.
/// </summary>
public static class ConfigExtensions
{
    public static string GetDatabaseConnectionString(this IConfiguration configuration)
    {
        var url = configuration["DATABASE_URL"];
        if (!string.IsNullOrEmpty(url))
            return url;
        return configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public static string GetFrontendUrl(this IConfiguration configuration)
    {
        return configuration["FRONTEND_URL"] ?? configuration["App:FrontendUrl"] ?? "";
    }

    public static string GetSiteName(this IConfiguration configuration)
    {
        return configuration["SITE_NAME"] ?? configuration["App:SiteName"] ?? "BFG";
    }
}
