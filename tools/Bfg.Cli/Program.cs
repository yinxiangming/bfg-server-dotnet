using Bfg.Cli.Commands;
using Bfg.Cli.Infrastructure;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

// Match Bfg.Api: do not overwrite DATABASE_URL from shell.
Env.NoClobber().TraversePath().Load();

var config = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

if (args.Length == 0 || args[0] is "-h" or "--help" or "help")
{
    PrintRootHelp();
    return args.Length == 0 ? 1 : 0;
}

try
{
    return args[0].ToLowerInvariant() switch
    {
        "workspace" => await WorkspaceCommands.RunAsync(args[1..], config, CancellationToken.None),
        _ => UnknownCommand(args[0])
    };
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.Message);
    return 1;
}

static int UnknownCommand(string name)
{
    Console.Error.WriteLine($"Unknown command: {name}");
    PrintRootHelp();
    return 1;
}

static void PrintRootHelp()
{
    Console.WriteLine("bfg-cli — BFG .NET database / ops utilities");
    Console.WriteLine();
    Console.WriteLine("Usage: bfg-cli <command> [options]");
    Console.WriteLine();
    Console.WriteLine("Commands:");
    Console.WriteLine("  workspace   Tenant / workspace operations (see: bfg-cli workspace --help)");
    Console.WriteLine();
    Console.WriteLine("Database: set DATABASE_URL or ConnectionStrings__DefaultConnection (same as Bfg.Api).");
}
