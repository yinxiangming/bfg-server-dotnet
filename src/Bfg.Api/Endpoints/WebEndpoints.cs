using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Common;
using Bfg.Core.Web;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class WebEndpoints
{
    public static void MapWebEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/web").WithTags("Web").RequireAuthorization();

        group.MapGet("/sites", ListSites);
        group.MapPost("/sites/", CreateSite);
        group.MapGet("/sites/{id:int}", GetSite);
        group.MapGet("/themes", ListThemes);
        group.MapGet("/languages", ListLanguages);
        group.MapGet("/pages", ListPages);
        group.MapPost("/pages/", CreatePage);
        group.MapGet("/pages/{id:int}", GetPage);
        group.MapGet("/inquiries", ListInquiries);
        group.MapPost("/media/", UploadMedia);
        group.MapGet("/blocks/types/", BlockTypes);
        group.MapPost("/blocks/validate/", BlockValidate);
        group.MapPost("/newsletter/unsubscribe/", NewsletterUnsubscribe);
    }

    private static async Task<IResult> ListSites(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebSites.AsNoTracking().Where(s => s.IsActive);
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.OrderBy(s => s.Name).Select(s => new { id = s.Id, name = s.Name, domain = s.Domain, site_title = s.SiteTitle, default_language = s.DefaultLanguage, workspace = s.WorkspaceId, is_active = s.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateSite(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<SiteCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var s = new Site
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Domain = body.domain ?? "",
            SiteTitle = body.site_title ?? "",
            DefaultLanguage = body.default_language ?? "en",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebSites.Add(s);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/sites/", new { id = s.Id, name = s.Name, domain = s.Domain, site_title = s.SiteTitle, workspace = s.WorkspaceId });
    }

    private static async Task<IResult> GetSite(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.WebSites.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        return Results.Ok(new { id = s.Id, name = s.Name, domain = s.Domain, workspace = s.WorkspaceId });
    }

    private static async Task<IResult> CreatePage(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<PageCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var p = new Page
        {
            WorkspaceId = wid.Value,
            Title = body.title ?? "",
            Slug = body.slug ?? "",
            Content = body.content ?? "",
            Status = body.status ?? "published",
            Language = body.language ?? "en",
            CreatedById = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebPages.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/pages/", new { id = p.Id, title = p.Title, slug = p.Slug, content = p.Content, status = p.Status, language = p.Language });
    }

    private static async Task<IResult> GetPage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { id = p.Id, title = p.Title, slug = p.Slug, content = p.Content, status = p.Status });
    }

    private static async Task<IResult> UploadMedia(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();

        string fileName, fileType, fileUrl;
        // Support both JSON and multipart
        if (ctx.Request.ContentType != null && ctx.Request.ContentType.Contains("multipart"))
        {
            var file = ctx.Request.Form.Files.GetFile("file");
            fileName = ctx.Request.Form["file_name"].ToString() ?? file?.FileName ?? "upload";
            fileType = ctx.Request.Form["file_type"].ToString() ?? ctx.Request.Form["mime_type"].ToString() ?? "image";
            fileUrl = ctx.Request.Form["file_url"].ToString() ?? "";
        }
        else
        {
            var body = await ctx.Request.ReadFromJsonAsync<MediaCreateBody>(ct);
            fileName = body?.file_name ?? "upload";
            fileType = body?.mime_type ?? body?.file_type ?? "image";
            fileUrl = body?.file_url ?? "";
        }

        var media = new Media
        {
            WorkspaceId = wid.Value,
            File = fileUrl.Length > 0 ? fileUrl : fileName,
            MediaType = fileType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Media.Add(media);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/media/", new { id = media.Id, file_name = fileName, file_url = fileUrl, mime_type = fileType });
    }

    private sealed record MediaCreateBody(string? file_name, string? file_url, string? mime_type, string? file_type);

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

    private sealed record SiteCreateBody(string? name, string? domain, string? site_title, string? default_language);
    private sealed record PageCreateBody(string? title, string? slug, string? content, string? status, string? language);
}
