using Bfg.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql.EntityFrameworkCore.PostgreSQL;

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
            .AddEnvironmentVariables()
            .Build();
        var conn = config["DATABASE_URL"] ?? config.GetConnectionString("DefaultConnection") ?? "Host=localhost;Database=bfg;Username=postgres;Password=postgres";
        var options = new DbContextOptionsBuilder<BfgDbContext>()
            .UseNpgsql(conn, b => b.MigrationsAssembly("Bfg.Api"))
            .Options;
        return new BfgDbContext(options);
    }
}
