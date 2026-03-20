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
        if (string.IsNullOrEmpty(url))
            return configuration.GetConnectionString("DefaultConnection") ?? "";
        if (url.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
            return MySqlUrlToConnectionString(url);
        return url;
    }

    /// <summary>
    /// Django-style DATABASE_URL (mysql://user:pass@host:port/db) to Pomelo connection string.
    /// </summary>
    private static string MySqlUrlToConnectionString(string mysqlUrl)
    {
        if (!Uri.TryCreate(mysqlUrl, UriKind.Absolute, out var u)
            || !string.Equals(u.Scheme, "mysql", StringComparison.OrdinalIgnoreCase))
            return mysqlUrl;
        var userInfo = (u.UserInfo ?? "").Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var db = u.AbsolutePath.TrimStart('/');
        var port = u.Port > 0 ? u.Port : 3306;
        return $"Server={u.Host};Port={port};Database={db};User={user};Password={pass};";
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
