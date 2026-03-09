using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Web;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class WebEndpoints
{
    public static void MapWebEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/web").WithTags("Web").RequireAuthorization();

        group.MapGet("/sites", ListSites);
        group.MapGet("/themes", ListThemes);
        group.MapGet("/languages", ListLanguages);
        group.MapGet("/pages", ListPages);
        group.MapGet("/inquiries", ListInquiries);
        group.MapGet("/blocks/types/", BlockTypes);
        group.MapPost("/blocks/validate/", BlockValidate);
        group.MapPost("/newsletter/unsubscribe/", NewsletterUnsubscribe);
    }

    private static async Task<IResult> ListSites(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebSites.AsNoTracking().Where(s => s.IsActive);
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.OrderBy(s => s.Name).Select(s => new { id = s.Id, name = s.Name, domain = s.Domain, is_active = s.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListThemes(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebThemes.AsNoTracking().Where(t => t.IsActive);
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.OrderBy(t => t.Name).Select(t => new { id = t.Id, name = t.Name, code = t.Code }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListLanguages(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebLanguages.AsNoTracking();
        if (wid.HasValue) query = query.Where(l => l.WorkspaceId == wid.Value);
        var list = await query.OrderBy(l => l.SortOrder).Select(l => new { id = l.Id, code = l.Code, name = l.Name, is_default = l.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListPages(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebPages.AsNoTracking();
        if (wid.HasValue) query = query.Where(p => p.WorkspaceId == wid.Value);
        var list = await query.OrderBy(p => p.Title).Select(p => new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListInquiries(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebInquiries.AsNoTracking();
        if (wid.HasValue) query = query.Where(i => i.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(i => i.CreatedAt).Select(i => new { id = i.Id, name = i.Name, email = i.Email, subject = i.Subject, status = i.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static IResult BlockTypes() => Results.Ok(new object[] { });

    private static IResult BlockValidate() => Results.Ok(new { valid = true });

    private static IResult NewsletterUnsubscribe() => Results.Ok(new { detail = "Unsubscribed." });
}
