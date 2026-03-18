using Bfg.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Bfg.Api;

/// <summary>
/// Design-time factory for EF migrations (uses env or appsettings connection).
/// </summary>
public class BfgDbContextFactory : IDesignTimeDbContextFactory<BfgDbContext>
{
    public BfgDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
        var conn = config["DATABASE_URL"] ?? config.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=bfg-dotnet;User=root;Password=;";
        var options = new DbContextOptionsBuilder<BfgDbContext>()
            .UseMySql(conn, ServerVersion.Parse("8.0.21"), b => b.MigrationsAssembly("Bfg.Api"))
            .Options;
        return new BfgDbContext(options);
    }
}
