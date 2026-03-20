using Microsoft.Extensions.Configuration;

namespace Bfg.Cli.Infrastructure;

/// <summary>
/// Resolves DB connection string the same way as Bfg.Api (DATABASE_URL, mysql:// URL, DefaultConnection).
/// </summary>
public static class DatabaseConfig
{
    public static string GetConnectionString(IConfiguration configuration)
    {
        var url = configuration["DATABASE_URL"];
        if (string.IsNullOrEmpty(url))
            return configuration.GetConnectionString("DefaultConnection") ?? "";
        if (url.StartsWith("mysql://", StringComparison.OrdinalIgnoreCase))
            return MySqlUrlToConnectionString(url);
        return url;
    }

    private static string MySqlUrlToConnectionString(string mysqlUrl)
    {
        if (!Uri.TryCreate(mysqlUrl, UriKind.Absolute, out var u)
            || !string.Equals(u.Scheme, "mysql", StringComparison.OrdinalIgnoreCase))
            return mysqlUrl;
        var userInfo = (u.UserInfo ?? "").Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
        var database = u.AbsolutePath.TrimStart('/');
        var port = u.Port > 0 ? u.Port : 3306;
        return $"Server={u.Host};Port={port};Database={database};User={user};Password={pass};";
    }
}
