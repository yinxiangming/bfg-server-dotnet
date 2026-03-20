using Bfg.Cli.Infrastructure;
using Bfg.Cli.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Bfg.Cli.Commands;

internal static class WorkspaceCommands
{
    internal static async Task<int> RunAsync(string[] args, IConfiguration config, CancellationToken ct)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintWorkspaceHelp();
            return args.Length == 0 ? 1 : 0;
        }

        switch (args[0].ToLowerInvariant())
        {
            case "purge":
                return await RunPurgeAsync(args[1..], config, ct);
            case "purge-all":
                return await RunPurgeAllAsync(args[1..], config, ct);
            default:
                Console.Error.WriteLine($"Unknown workspace subcommand: {args[0]}");
                PrintWorkspaceHelp();
                return 1;
        }
    }

    private static async Task<int> RunPurgeAsync(string[] args, IConfiguration config, CancellationToken ct)
    {
        var rest = args.Where(a => !string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase)).ToArray();
        var dryRun = args.Any(a => string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase));

        if (rest.Length == 0 || !int.TryParse(rest[0], out var workspaceId) || IsHelp(rest[0]))
        {
            Console.Error.WriteLine("Usage: bfg-cli workspace purge <workspaceId> [--dry-run]");
            return 1;
        }

        var conn = DatabaseConfig.GetConnectionString(config);
        if (string.IsNullOrWhiteSpace(conn))
        {
            Console.Error.WriteLine("Missing database: set DATABASE_URL or ConnectionStrings__DefaultConnection.");
            return 1;
        }

        await using var db = BfgDbContextFactory.Create(conn);

        var ws = await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
        if (ws == null)
        {
            Console.Error.WriteLine($"Workspace id={workspaceId} not found.");
            return 1;
        }

        Console.WriteLine($"Target workspace: {workspaceId} ({ws.Name}, slug={ws.Slug})");

        if (dryRun)
        {
            Console.WriteLine("Dry-run: no changes. Remove --dry-run to execute purge.");
            return 0;
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await WorkspacePurgeService.PurgeAsync(db, workspaceId, ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        Console.WriteLine($"Purged workspace id={workspaceId}.");
        return 0;
    }

    private static async Task<int> RunPurgeAllAsync(string[] args, IConfiguration config, CancellationToken ct)
    {
        var dryRun = args.Any(a => string.Equals(a, "--dry-run", StringComparison.OrdinalIgnoreCase));
        var confirm = args.Any(a => string.Equals(a, "--confirm", StringComparison.OrdinalIgnoreCase));

        var conn = DatabaseConfig.GetConnectionString(config);
        if (string.IsNullOrWhiteSpace(conn))
        {
            Console.Error.WriteLine("Missing database: set DATABASE_URL or ConnectionStrings__DefaultConnection.");
            return 1;
        }

        await using var db = BfgDbContextFactory.Create(conn);

        var list = await db.Workspaces.AsNoTracking().OrderBy(w => w.Id).Select(w => new { w.Id, w.Name, w.Slug }).ToListAsync(ct);
        if (list.Count == 0)
        {
            Console.WriteLine("No workspaces in database.");
            return 0;
        }

        Console.WriteLine($"Found {list.Count} workspace(s):");
        foreach (var w in list)
            Console.WriteLine($"  id={w.Id}  name={w.Name}  slug={w.Slug}");

        Console.WriteLine();
        Console.WriteLine("common_user is never deleted (superadmin and other accounts remain).");
        Console.WriteLine("Per-workspace StaffMember/Customer rows are removed; DefaultWorkspaceId is cleared when needed.");

        if (dryRun)
        {
            Console.WriteLine("Dry-run: no changes. Run with --confirm to execute (omit --dry-run).");
            return 0;
        }

        if (!confirm)
        {
            Console.Error.WriteLine("Refusing to purge all workspaces without --confirm (use --dry-run to preview only).");
            return 1;
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        try
        {
            await WorkspacePurgeService.PurgeAllWorkspacesAsync(db, ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        Console.WriteLine($"Purged all {list.Count} workspace(s).");
        return 0;
    }

    private static void PrintWorkspaceHelp()
    {
        Console.WriteLine("Workspace commands:");
        Console.WriteLine("  bfg-cli workspace purge <id> [--dry-run]     Remove tenant data for one workspace.");
        Console.WriteLine("  bfg-cli workspace purge-all [--dry-run]        List / preview purge of every workspace.");
        Console.WriteLine("  bfg-cli workspace purge-all --confirm          Execute purge of every workspace (dangerous).");
    }

    private static bool IsHelp(string s) =>
        s is "-h" or "--help" or "help";
}
