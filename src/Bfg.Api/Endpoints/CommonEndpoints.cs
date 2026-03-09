using System.Security.Claims;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class CommonEndpoints
{
    public static void MapCommonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Common").RequireAuthorization();

        group.MapGet("/workspaces", ListWorkspaces);
        group.MapGet("/customers", ListCustomers);
        group.MapGet("/addresses", ListAddresses);
        group.MapGet("/settings", ListSettings);
        group.MapGet("/email-configs", ListEmailConfigs);
        group.MapGet("/users", ListUsers);
        group.MapGet("/customer-segments", ListCustomerSegments);
        group.MapGet("/customer-tags", ListCustomerTags);
        group.MapGet("/staff-roles", ListStaffRoles);
        group.MapGet("/me", Me);
        group.MapGet("/options/", Options);
        group.MapGet("/countries/", Countries);
    }

    private static async Task<IResult> ListWorkspaces(BfgDbContext db, CancellationToken ct)
    {
        var list = await db.Workspaces.AsNoTracking().Where(w => w.IsActive).OrderBy(w => w.Name)
            .Select(w => new { id = w.Id, name = w.Name, slug = w.Slug, domain = w.Domain, email = w.Email, phone = w.Phone, is_active = w.IsActive, settings = w.Settings, created_at = w.CreatedAt, updated_at = w.UpdatedAt })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> Me(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        return Results.Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            first_name = user.FirstName,
            last_name = user.LastName,
            phone = user.Phone,
            avatar = user.Avatar,
            default_workspace = user.DefaultWorkspaceId,
            language = user.Language,
            timezone_name = user.TimezoneName,
            is_staff = user.IsStaff,
            is_active = user.IsActive,
            date_joined = user.DateJoined,
            updated_at = user.UpdatedAt
        });
    }

    private static IResult Options()
    {
        return Results.Ok(new { });
    }

    private static IResult Countries()
    {
        var countries = new[] { new { code = "NZ", name = "New Zealand" }, new { code = "AU", name = "Australia" }, new { code = "US", name = "United States" }, new { code = "CN", name = "China" } };
        return Results.Ok(countries);
    }

    private static async Task<IResult> ListCustomers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Customers.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(c => c.CreatedAt).Select(c => new { id = c.Id, customer_number = c.CustomerNumber, is_active = c.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListAddresses(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Addresses.AsNoTracking();
        if (wid.HasValue) query = query.Where(a => a.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(a => a.CreatedAt).Select(a => new { id = a.Id, full_name = a.FullName, city = a.City, country = a.Country }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListSettings(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.Settings.AsNoTracking().Where(s => wid == null || s.WorkspaceId == wid.Value).Select(s => new { id = s.Id, workspace_id = s.WorkspaceId, site_name = s.SiteName }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListEmailConfigs(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.EmailConfigs.AsNoTracking();
        if (wid.HasValue) query = query.Where(e => e.WorkspaceId == wid.Value);
        var list = await query.Select(e => new { id = e.Id, name = e.Name, backend_type = e.BackendType, is_default = e.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListUsers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Users.AsNoTracking();
        if (wid.HasValue) query = query.Where(u => u.DefaultWorkspaceId == wid.Value);
        var list = await query.Select(u => new { id = u.Id, username = u.Username, email = u.Email, is_active = u.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListCustomerSegments(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.CustomerSegments.AsNoTracking();
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.Select(s => new { id = s.Id, name = s.Name, is_active = s.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListCustomerTags(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.CustomerTags.AsNoTracking();
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.Select(t => new { id = t.Id, name = t.Name }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListStaffRoles(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.StaffRoles.AsNoTracking().Where(r => r.IsActive);
        if (wid.HasValue) query = query.Where(r => r.WorkspaceId == wid.Value);
        var list = await query.Select(r => new { id = r.Id, name = r.Name, code = r.Code }).ToListAsync(ct);
        return Results.Ok(list);
    }
}
