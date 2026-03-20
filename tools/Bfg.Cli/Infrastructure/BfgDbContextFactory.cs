using Bfg.Core;
using EFCore.NamingConventions;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Bfg.Cli.Infrastructure;

public static class BfgDbContextFactory
{
    public static BfgDbContext Create(string connectionString)
    {
        var options = new DbContextOptionsBuilder<BfgDbContext>()
            .UseMySql(connectionString, ServerVersion.Parse("8.0.21-mysql"))
            .UseSnakeCaseNamingConvention()
            .Options;
        return new BfgDbContext(options);
    }
}
