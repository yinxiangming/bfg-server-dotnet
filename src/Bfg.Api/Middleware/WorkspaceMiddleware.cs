using Bfg.Core;
using Bfg.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Middleware;

/// <summary>
/// Resolves current workspace from Host or X-Workspace-Id and sets it in HttpContext.
/// Matches Django bfg.common.middleware.WorkspaceMiddleware.
/// </summary>
public class WorkspaceMiddleware
{
    private readonly RequestDelegate _next;
    private const string WorkspaceKey = "Workspace";
    private const string WorkspaceIdKey = "WorkspaceId";

    public static readonly PathString[] SkipPaths = { "/api/docs", "/api/schema", "/api/v1/auth/" };

    public WorkspaceMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, BfgDbContext db)
    {
        if (ShouldSkip(context))
        {
            await _next(context);
            return;
        }

        var workspaceIdHeader = context.Request.Headers["X-Workspace-Id"].FirstOrDefault();
        Workspace? workspace = null;

        if (!string.IsNullOrEmpty(workspaceIdHeader) && int.TryParse(workspaceIdHeader, out var id))
            workspace = await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id && w.IsActive);

        if (workspace == null)
        {
            var host = context.Request.Host.Value;
            workspace = await db.Workspaces.AsNoTracking()
                .Where(w => w.Domain == host && w.IsActive)
                .OrderBy(w => w.Id)
                .FirstOrDefaultAsync();
        }

        if (workspace != null)
        {
            context.Items[WorkspaceKey] = workspace;
            context.Items[WorkspaceIdKey] = workspace.Id;
        }

        await _next(context);
    }

    private static bool ShouldSkip(HttpContext context)
    {
        var path = context.Request.Path;
        foreach (var skip in SkipPaths)
            if (path.StartsWithSegments(skip, StringComparison.OrdinalIgnoreCase))
                return true;
        return false;
    }

    public static Workspace? GetWorkspace(HttpContext context) =>
        context.Items[WorkspaceKey] as Workspace;

    public static int? GetWorkspaceId(HttpContext context) =>
        context.Items[WorkspaceIdKey] as int?;
}
