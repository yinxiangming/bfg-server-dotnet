using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class PlatformEndpoints
{
    public static void MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/v1/platform").WithTags("Platform").RequireAuthorization();
        var pub = app.MapGroup("/api/v1/platform").WithTags("Platform");

        auth.MapGet("/workspaces/me/", ListMyWorkspaces);
        auth.MapGet("/workspaces/", ListWorkspaces);
        auth.MapPost("/workspaces/", CreateWorkspace);
        auth.MapGet("/workspaces/{id:int}/", GetWorkspace);
        auth.MapPost("/workspaces/{id:int}/suspend/", SuspendWorkspace);
        auth.MapPost("/workspaces/{id:int}/resume/", ResumeWorkspace);
        auth.MapGet("/workspaces/{id:int}/subscription/", GetWorkspaceSubscription);
        auth.MapPost("/auth/token-exchange/", TokenExchange);

        pub.MapGet("/plans/", ListPlans);
        pub.MapGet("/auth/sso-check/", SsoCheck);
    }

    private static async Task<IResult> ListMyWorkspaces(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userId = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value, ct);
        if (user == null) return Results.Unauthorized();

        List<WorkspaceListItem> workspaces;
        if (user.IsSuperuser)
        {
            workspaces = await db.Workspaces.AsNoTracking()
                .Where(w => w.IsActive)
                .OrderBy(w => w.Name)
                .Select(w => new WorkspaceListItem(
                    w.Id,
                    w.Name,
                    w.Slug,
                    w.Uuid,
                    w.Email,
                    w.IsActive,
                    w.Slug == "admin"))
                .ToListAsync(ct);
        }
        else
        {
            workspaces = await db.StaffMembers.AsNoTracking()
                .Where(sm => sm.UserId == userId.Value && sm.IsActive && sm.Workspace.IsActive)
                .OrderBy(sm => sm.Workspace.Name)
                .Select(sm => new WorkspaceListItem(
                    sm.Workspace.Id,
                    sm.Workspace.Name,
                    sm.Workspace.Slug,
                    sm.Workspace.Uuid,
                    sm.Workspace.Email,
                    sm.Workspace.IsActive,
                    sm.Workspace.Slug == "admin"))
                .Distinct()
                .ToListAsync(ct);
        }

        return Results.Ok(new
        {
            is_platform_admin = user.IsSuperuser || user.IsStaff,
            workspaces = workspaces.Select(w => new
            {
                id = w.Id,
                name = w.Name,
                slug = w.Slug,
                uuid = w.Uuid,
                email = w.Email,
                is_active = w.IsActive,
                is_platform = w.IsPlatform
            })
        });
    }

    private static async Task<IResult> ListWorkspaces(BfgDbContext db, HttpRequest req, CancellationToken ct)
    {
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Workspaces.AsNoTracking().OrderBy(w => w.Name);
        var total = await query.CountAsync(ct);
        var list = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new
            {
                id = w.Id,
                name = w.Name,
                uuid = w.Uuid,
                slug = w.Slug,
                email = w.Email,
                phone = w.Phone,
                is_active = w.IsActive,
                settings = w.Settings,
                created_at = w.CreatedAt,
                updated_at = w.UpdatedAt,
                is_platform = w.Slug == "admin"
            })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateWorkspace(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<WorkspaceCreateBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.slug) && string.IsNullOrWhiteSpace(body.name))
            return Results.BadRequest();

        var slug = body.slug ?? Slugify(body.name ?? "workspace");
        if (await db.Workspaces.AnyAsync(w => w.Slug == slug, ct))
            return Results.BadRequest(new { slug = new[] { "Workspace with this slug already exists." } });

        var now = DateTime.UtcNow;
        var workspace = new Workspace
        {
            Name = body.name ?? slug,
            Uuid = Guid.NewGuid().ToString("N"),
            Slug = slug,
            Email = body.email ?? string.Empty,
            Phone = string.Empty,
            IsActive = true,
            Settings = "{}",
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Workspaces.Add(workspace);
        await db.SaveChangesAsync(ct);

        db.Settings.Add(new Settings
        {
            WorkspaceId = workspace.Id,
            SiteName = workspace.Name,
            DefaultLanguage = "en",
            DefaultCurrency = "NZD",
            UpdatedAt = now
        });

        var adminRole = new StaffRole
        {
            WorkspaceId = workspace.Id,
            Name = "Admin",
            Code = "admin",
            Description = "Administrator",
            Permissions = "{}",
            IsSystem = true,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.StaffRoles.Add(adminRole);
        await db.SaveChangesAsync(ct);

        var userId = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (userId.HasValue)
        {
            db.StaffMembers.Add(new StaffMember
            {
                WorkspaceId = workspace.Id,
                UserId = userId.Value,
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                var logger = ctx.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("PlatformEndpoints");
                logger.LogError(ex,
                    "Failed to create platform staff member. workspace_id={WorkspaceId} user_id={UserId} role_id={RoleId} inner={InnerMessage}",
                    workspace.Id,
                    userId.Value,
                    adminRole.Id,
                    ex.InnerException?.Message);
                throw;
            }
        }

        return Results.Created($"/api/v1/platform/workspaces/{workspace.Id}/", new
        {
            id = workspace.Id,
            name = workspace.Name,
            slug = workspace.Slug,
            is_active = workspace.IsActive,
            is_platform = workspace.Slug == "admin",
            created_at = workspace.CreatedAt,
            updated_at = workspace.UpdatedAt
        });
    }

    private static async Task<IResult> GetWorkspace(BfgDbContext db, int id, CancellationToken ct)
    {
        var workspace = await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);
        if (workspace == null) return Results.NotFound();
        return Results.Ok(new
        {
            id = workspace.Id,
            name = workspace.Name,
            uuid = workspace.Uuid,
            slug = workspace.Slug,
            email = workspace.Email,
            phone = workspace.Phone,
            is_active = workspace.IsActive,
            settings = workspace.Settings,
            created_at = workspace.CreatedAt,
            updated_at = workspace.UpdatedAt,
            is_platform = workspace.Slug == "admin"
        });
    }

    private static async Task<IResult> SuspendWorkspace(BfgDbContext db, int id, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (workspace == null) return Results.NotFound();
        workspace.IsActive = false;
        workspace.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = workspace.Id, is_active = workspace.IsActive });
    }

    private static async Task<IResult> ResumeWorkspace(BfgDbContext db, int id, CancellationToken ct)
    {
        var workspace = await db.Workspaces.FirstOrDefaultAsync(w => w.Id == id, ct);
        if (workspace == null) return Results.NotFound();
        workspace.IsActive = true;
        workspace.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = workspace.Id, is_active = workspace.IsActive });
    }

    private static async Task<IResult> GetWorkspaceSubscription(BfgDbContext db, int id, CancellationToken ct)
    {
        var exists = await db.Workspaces.AsNoTracking().AnyAsync(w => w.Id == id, ct);
        if (!exists) return Results.NotFound();
        return Results.Ok(new { subscription = (object?)null });
    }

    private static async Task<IResult> TokenExchange(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userId = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();

        var body = await ctx.Request.ReadFromJsonAsync<TokenExchangeBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.workspace_id))
            return Results.BadRequest(new { workspace_id = new[] { "This field is required." } });

        var workspace = await ResolveWorkspaceAsync(db, body.workspace_id, ct);
        if (workspace == null) return Results.NotFound();

        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value, ct);
        if (user == null) return Results.Unauthorized();

        var jwt = ctx.RequestServices.GetRequiredService<Bfg.Api.Services.JwtService>();
        return Results.Ok(new
        {
            embedded = true,
            workspace_token = jwt.GenerateAccessToken(user),
            workspace = new
            {
                id = workspace.Id,
                slug = workspace.Slug,
                name = workspace.Name,
                is_active = workspace.IsActive,
                is_platform = workspace.Slug == "admin"
            }
        });
    }

    private static IResult ListPlans()
        => Results.Ok(Array.Empty<object>());

    private static IResult SsoCheck(HttpRequest req)
        => Results.Ok(new { sso_enabled = false, domain = req.Query["domain"].ToString() });

    private static async Task<Workspace?> ResolveWorkspaceAsync(BfgDbContext db, string workspaceId, CancellationToken ct)
    {
        if (int.TryParse(workspaceId, out var id))
            return await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == id, ct);
        return await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Slug == workspaceId, ct);
    }

    private static string Slugify(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "workspace";
        var slug = new string(name.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray()).Trim();
        slug = string.Join("-", slug.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries));
        return slug.Length > 0 ? slug : "workspace";
    }

    private sealed record WorkspaceCreateBody(string? name, string? slug, string? email);
    private sealed record TokenExchangeBody(string? workspace_id);
    private sealed record WorkspaceListItem(int Id, string Name, string Slug, string Uuid, string Email, bool IsActive, bool IsPlatform);
}
