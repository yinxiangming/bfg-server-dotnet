using Bfg.Api.Auth;
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

        // Sites
        group.MapGet("/sites", ListSites);
        group.MapPost("/sites/", CreateSite);
        group.MapGet("/sites/{id:int}", GetSite);
        group.MapPatch("/sites/{id:int}", PatchSite);
        group.MapDelete("/sites/{id:int}", DeleteSite);
        group.MapPost("/sites/{id:int}/set_default", SetSiteDefault);

        // Themes
        group.MapGet("/themes", ListThemes);
        group.MapPost("/themes/", CreateTheme);
        group.MapGet("/themes/{id:int}", GetTheme);
        group.MapPatch("/themes/{id:int}", PatchTheme);
        group.MapDelete("/themes/{id:int}", DeleteTheme);

        // Languages
        group.MapGet("/languages", ListLanguages);
        group.MapPost("/languages/", CreateLanguage);
        group.MapGet("/languages/{id:int}", GetLanguage);
        group.MapPatch("/languages/{id:int}", PatchLanguage);
        group.MapDelete("/languages/{id:int}", DeleteLanguage);
        group.MapPost("/languages/{id:int}/set_default", SetLanguageDefault);

        // Pages
        group.MapGet("/pages", ListPages);
        group.MapPost("/pages/", CreatePage);
        group.MapGet("/pages/tree", GetPageTree);
        group.MapGet("/pages/{id:int}", GetPage);
        group.MapPatch("/pages/{id:int}", PatchPage);
        group.MapDelete("/pages/{id:int}", DeletePage);
        group.MapPost("/pages/{id:int}/publish", PublishPage);

        // Posts
        group.MapGet("/posts", ListPosts);
        group.MapPost("/posts/", CreatePost);
        group.MapGet("/posts/{id:int}", GetPost);
        group.MapPatch("/posts/{id:int}", PatchPost);
        group.MapDelete("/posts/{id:int}", DeletePost);
        group.MapPost("/posts/{id:int}/publish", PublishPost);

        // Menus
        group.MapGet("/menus", ListMenus);
        group.MapPost("/menus/", CreateMenu);
        group.MapGet("/menus/{id:int}", GetMenu);
        group.MapPatch("/menus/{id:int}", PatchMenu);
        group.MapDelete("/menus/{id:int}", DeleteMenu);

        // Menu Items
        group.MapGet("/menu-items", ListMenuItems);
        group.MapPost("/menu-items/", CreateMenuItem);
        group.MapPatch("/menu-items/{id:int}", PatchMenuItem);
        group.MapDelete("/menu-items/{id:int}", DeleteMenuItem);

        // Tags
        group.MapGet("/tags", ListTags);
        group.MapPost("/tags/", CreateTag);
        group.MapGet("/tags/{id:int}", GetTag);
        group.MapPatch("/tags/{id:int}", PatchTag);
        group.MapDelete("/tags/{id:int}", DeleteTag);

        // Web Categories
        group.MapGet("/categories", ListWebCategories);
        group.MapPost("/categories/", CreateWebCategory);
        group.MapGet("/categories/{id:int}", GetWebCategory);
        group.MapPatch("/categories/{id:int}", PatchWebCategory);
        group.MapDelete("/categories/{id:int}", DeleteWebCategory);

        // Newsletter
        group.MapGet("/newsletter/subscriptions", ListNewsletterSubscriptions);
        group.MapPost("/newsletter/subscriptions/", CreateNewsletterSubscription);
        group.MapDelete("/newsletter/subscriptions/{id:int}", DeleteNewsletterSubscription);
        group.MapGet("/newsletter/templates", ListNewsletterTemplates);
        group.MapPost("/newsletter/templates/", CreateNewsletterTemplate);
        group.MapGet("/newsletter/templates/{id:int}", GetNewsletterTemplate);
        group.MapPatch("/newsletter/templates/{id:int}", PatchNewsletterTemplate);
        group.MapGet("/newsletter/sends", ListNewsletterSends);
        group.MapPost("/newsletter/sends/", CreateNewsletterSend);
        group.MapGet("/newsletter/sends/{id:int}", GetNewsletterSend);

        // Inquiries
        group.MapGet("/inquiries", ListInquiries);
        group.MapGet("/inquiries/stats", GetInquiryStats);
        group.MapGet("/inquiries/{id:int}", GetInquiry);
        group.MapPatch("/inquiries/{id:int}", PatchInquiry);
        group.MapDelete("/inquiries/{id:int}", DeleteInquiry);

        // Bookings
        group.MapGet("/bookings", ListBookings);
        group.MapPost("/bookings/", CreateBooking);
        group.MapGet("/bookings/{id:int}", GetBooking);
        group.MapPatch("/bookings/{id:int}", PatchBooking);
        group.MapPost("/bookings/{id:int}/cancel", CancelBooking);
        group.MapPost("/bookings/{id:int}/confirm", ConfirmBooking);

        // Time Slots
        group.MapGet("/timeslots", ListTimeSlots);
        group.MapPost("/timeslots/", CreateTimeSlot);
        group.MapGet("/timeslots/available", GetAvailableTimeSlots);
        group.MapGet("/timeslots/{id:int}", GetTimeSlot);
        group.MapPatch("/timeslots/{id:int}", PatchTimeSlot);

        // Feedback
        group.MapPost("/feedback/", SubmitFeedback);

        // Media / Blocks / Newsletter (existing)
        group.MapPost("/media/", UploadMedia);
        group.MapGet("/blocks/types/", BlockTypes);
        group.MapPost("/blocks/validate/", BlockValidate);
        group.MapPost("/newsletter/unsubscribe/", NewsletterUnsubscribe);
    }

    // ── Sites ─────────────────────────────────────────────────────────────────

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

    private static async Task<IResult> PatchSite(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.WebSites.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<SitePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) s.Name = body.name;
            if (body.domain != null) s.Domain = body.domain;
            if (body.is_active.HasValue) s.IsActive = body.is_active.Value;
            if (body.is_default.HasValue) s.IsDefault = body.is_default.Value;
            s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = s.Id, name = s.Name, domain = s.Domain, is_active = s.IsActive, is_default = s.IsDefault });
    }

    private static async Task<IResult> DeleteSite(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.WebSites.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        s.IsActive = false;
        s.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetSiteDefault(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.WebSites.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        // Clear existing default for this workspace
        var others = await db.WebSites.Where(x => x.WorkspaceId == s.WorkspaceId && x.IsDefault && x.Id != id).ToListAsync(ct);
        foreach (var o in others) { o.IsDefault = false; o.UpdatedAt = DateTime.UtcNow; }
        s.IsDefault = true;
        s.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = s.Id, is_default = s.IsDefault });
    }

    // ── Themes ────────────────────────────────────────────────────────────────

    private static async Task<IResult> ListThemes(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebThemes.AsNoTracking().Where(t => t.IsActive);
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.OrderBy(t => t.Name).Select(t => new { id = t.Id, name = t.Name, code = t.Code }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTheme(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<ThemeCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new Theme
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Code = body.code ?? "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebThemes.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/themes/", new { id = t.Id, name = t.Name, code = t.Code, workspace = t.WorkspaceId });
    }

    private static async Task<IResult> GetTheme(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.WebThemes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, name = t.Name, code = t.Code, is_active = t.IsActive, workspace = t.WorkspaceId });
    }

    private static async Task<IResult> PatchTheme(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.WebThemes.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<ThemePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) t.Name = body.name;
            if (body.code != null) t.Code = body.code;
            if (body.is_active.HasValue) t.IsActive = body.is_active.Value;
            t.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = t.Id, name = t.Name, code = t.Code, is_active = t.IsActive });
    }

    private static async Task<IResult> DeleteTheme(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.WebThemes.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        t.IsActive = false;
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Languages ─────────────────────────────────────────────────────────────

    private static async Task<IResult> ListLanguages(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebLanguages.AsNoTracking();
        if (wid.HasValue) query = query.Where(l => l.WorkspaceId == wid.Value);
        var list = await query.OrderBy(l => l.SortOrder).Select(l => new { id = l.Id, code = l.Code, name = l.Name, is_default = l.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateLanguage(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<LanguageCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var l = new Language
        {
            WorkspaceId = wid.Value,
            Code = body.code ?? "",
            Name = body.name ?? "",
            IsDefault = body.is_default ?? false,
            SortOrder = body.sort_order ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebLanguages.Add(l);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/languages/", new { id = l.Id, code = l.Code, name = l.Name, is_default = l.IsDefault, sort_order = l.SortOrder });
    }

    private static async Task<IResult> GetLanguage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var l = await db.WebLanguages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (l == null) return Results.NotFound();
        return Results.Ok(new { id = l.Id, code = l.Code, name = l.Name, is_default = l.IsDefault, sort_order = l.SortOrder });
    }

    private static async Task<IResult> PatchLanguage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var l = await db.WebLanguages.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (l == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<LanguagePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) l.Name = body.name;
            if (body.is_default.HasValue) l.IsDefault = body.is_default.Value;
            if (body.sort_order.HasValue) l.SortOrder = body.sort_order.Value;
            l.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = l.Id, code = l.Code, name = l.Name, is_default = l.IsDefault, sort_order = l.SortOrder });
    }

    private static async Task<IResult> DeleteLanguage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var l = await db.WebLanguages.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (l == null) return Results.NotFound();
        db.WebLanguages.Remove(l);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetLanguageDefault(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var l = await db.WebLanguages.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (l == null) return Results.NotFound();
        var others = await db.WebLanguages.Where(x => x.WorkspaceId == l.WorkspaceId && x.IsDefault && x.Id != id).ToListAsync(ct);
        foreach (var o in others) { o.IsDefault = false; o.UpdatedAt = DateTime.UtcNow; }
        l.IsDefault = true;
        l.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = l.Id, code = l.Code, is_default = l.IsDefault });
    }

    // ── Pages ─────────────────────────────────────────────────────────────────

    private static async Task<IResult> ListPages(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebPages.AsNoTracking();
        if (wid.HasValue) query = query.Where(p => p.WorkspaceId == wid.Value);
        var list = await query.OrderBy(p => p.Title).Select(p => new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreatePage(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        if (!AuthUser.TryGetUserId(ctx, out var userId)) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<PageCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var status = body.status ?? "published";
        var now = DateTime.UtcNow;
        var p = new Page
        {
            WorkspaceId = wid.Value,
            Title = body.title ?? "",
            Slug = body.slug ?? "",
            Content = body.content ?? "",
            Status = status,
            Language = body.language ?? "en",
            CreatedById = userId,
            PublishedAt = status == "published" ? now : null,
            CreatedAt = now,
            UpdatedAt = now
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

    private static async Task<IResult> PatchPage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPages.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PagePatchBody>(ct);
        if (body != null)
        {
            if (body.title != null) p.Title = body.title;
            if (body.content != null) p.Content = body.content;
            if (body.status != null) p.Status = body.status;
            if (body.slug != null) p.Slug = body.slug;
            if (body.language != null) p.Language = body.language;
            p.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status, language = p.Language });
    }

    private static async Task<IResult> DeletePage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPages.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        db.WebPages.Remove(p);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PublishPage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPages.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        p.Status = "published";
        p.PublishedAt = DateTime.UtcNow;
        p.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = p.Id, status = p.Status, published_at = p.PublishedAt });
    }

    private static async Task<IResult> GetPageTree(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebPages.AsNoTracking();
        if (wid.HasValue) query = query.Where(p => p.WorkspaceId == wid.Value);
        var pages = await query.Select(p => new PageTreeNode(p.Id, p.Title, p.Slug, p.Status, p.ParentId)).ToListAsync(ct);
        var lookup = pages.ToLookup(p => p.ParentId);
        List<object> BuildChildren(int? parentId) =>
            lookup[parentId].Select(p => (object)new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status, children = BuildChildren(p.Id) }).ToList();
        return Results.Ok(BuildChildren(null));
    }

    // ── Posts ─────────────────────────────────────────────────────────────────

    private static async Task<IResult> ListPosts(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebPosts.AsNoTracking();
        if (wid.HasValue) query = query.Where(p => p.WorkspaceId == wid.Value);
        if (req.Query.TryGetValue("status", out var statusVal)) query = query.Where(p => p.Status == statusVal.ToString());
        var list = await query.OrderByDescending(p => p.CreatedAt).Select(p => new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status, language = p.Language, created_at = p.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreatePost(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        if (!AuthUser.TryGetUserId(ctx, out var userId)) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<PostCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var status = body.status ?? "draft";
        var now = DateTime.UtcNow;
        var p = new Post
        {
            WorkspaceId = wid.Value,
            Title = body.title ?? "",
            Slug = body.slug ?? "",
            Content = body.content ?? "",
            Excerpt = body.excerpt ?? "",
            Status = status,
            Language = body.language ?? "en",
            CreatedById = userId,
            PublishedAt = status == "published" ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.WebPosts.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/posts/", new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status, language = p.Language });
    }

    private static async Task<IResult> GetPost(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPosts.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { id = p.Id, title = p.Title, slug = p.Slug, content = p.Content, excerpt = p.Excerpt, status = p.Status, language = p.Language, published_at = p.PublishedAt });
    }

    private static async Task<IResult> PatchPost(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPosts.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PostPatchBody>(ct);
        if (body != null)
        {
            if (body.title != null) p.Title = body.title;
            if (body.slug != null) p.Slug = body.slug;
            if (body.content != null) p.Content = body.content;
            if (body.excerpt != null) p.Excerpt = body.excerpt;
            if (body.status != null) p.Status = body.status;
            if (body.language != null) p.Language = body.language;
            p.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = p.Id, title = p.Title, slug = p.Slug, status = p.Status });
    }

    private static async Task<IResult> DeletePost(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPosts.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        db.WebPosts.Remove(p);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> PublishPost(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.WebPosts.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        p.Status = "published";
        p.PublishedAt = DateTime.UtcNow;
        p.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = p.Id, status = p.Status, published_at = p.PublishedAt });
    }

    // ── Menus ─────────────────────────────────────────────────────────────────

    private static async Task<IResult> ListMenus(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebMenus.AsNoTracking().Where(m => m.IsActive);
        if (wid.HasValue) query = query.Where(m => m.WorkspaceId == wid.Value);
        var list = await query.OrderBy(m => m.Name).Select(m => new { id = m.Id, name = m.Name, slug = m.Slug, location = m.Location, language = m.Language }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateMenu(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<MenuCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var m = new Menu
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Slug = body.slug ?? "",
            Location = body.location ?? "",
            Language = body.language ?? "en",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebMenus.Add(m);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/menus/", new { id = m.Id, name = m.Name, slug = m.Slug, location = m.Location, language = m.Language });
    }

    private static async Task<IResult> GetMenu(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.WebMenus.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        var items = await db.WebMenuItems.AsNoTracking().Where(i => i.MenuId == id).OrderBy(i => i.SortOrder).Select(i => new { id = i.Id, label = i.Label, url = i.Url, sort_order = i.SortOrder, parent_id = i.ParentId }).ToListAsync(ct);
        return Results.Ok(new { id = m.Id, name = m.Name, slug = m.Slug, location = m.Location, language = m.Language, items });
    }

    private static async Task<IResult> PatchMenu(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.WebMenus.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<MenuPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) m.Name = body.name;
            if (body.slug != null) m.Slug = body.slug;
            if (body.location != null) m.Location = body.location;
            if (body.language != null) m.Language = body.language;
            m.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = m.Id, name = m.Name, slug = m.Slug, location = m.Location, language = m.Language });
    }

    private static async Task<IResult> DeleteMenu(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.WebMenus.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        m.IsActive = false;
        m.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Menu Items ────────────────────────────────────────────────────────────

    private static async Task<IResult> ListMenuItems(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebMenuItems.AsNoTracking();
        if (req.Query.TryGetValue("menu_id", out var menuIdVal) && int.TryParse(menuIdVal, out var menuId))
            query = query.Where(i => i.MenuId == menuId);
        // Scope by workspace via menu join
        if (wid.HasValue)
        {
            var menuIds = await db.WebMenus.AsNoTracking().Where(m => m.WorkspaceId == wid.Value).Select(m => m.Id).ToListAsync(ct);
            query = query.Where(i => menuIds.Contains(i.MenuId));
        }
        var list = await query.OrderBy(i => i.SortOrder).Select(i => new { id = i.Id, menu_id = i.MenuId, label = i.Label, url = i.Url, sort_order = i.SortOrder, parent_id = i.ParentId }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateMenuItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<MenuItemCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var item = new MenuItem
        {
            MenuId = body.menu_id,
            Label = body.label ?? "",
            Url = body.url ?? "",
            SortOrder = body.sort_order ?? 0,
            ParentId = body.parent_id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebMenuItems.Add(item);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/menu-items/", new { id = item.Id, menu_id = item.MenuId, label = item.Label, url = item.Url, sort_order = item.SortOrder });
    }

    private static async Task<IResult> PatchMenuItem(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var item = await db.WebMenuItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<MenuItemPatchBody>(ct);
        if (body != null)
        {
            if (body.label != null) item.Label = body.label;
            if (body.url != null) item.Url = body.url;
            if (body.sort_order.HasValue) item.SortOrder = body.sort_order.Value;
            if (body.parent_id.HasValue) item.ParentId = body.parent_id.Value;
            item.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = item.Id, label = item.Label, url = item.Url, sort_order = item.SortOrder });
    }

    private static async Task<IResult> DeleteMenuItem(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var item = await db.WebMenuItems.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (item == null) return Results.NotFound();
        db.WebMenuItems.Remove(item);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Tags ──────────────────────────────────────────────────────────────────

    private static async Task<IResult> ListTags(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebTags.AsNoTracking();
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.OrderBy(t => t.Name).Select(t => new { id = t.Id, name = t.Name, slug = t.Slug, language = t.Language }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTag(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TagCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new WebTag
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Slug = body.slug ?? "",
            Language = body.language ?? "en",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebTags.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/tags/", new { id = t.Id, name = t.Name, slug = t.Slug, language = t.Language });
    }

    private static async Task<IResult> GetTag(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.WebTags.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, name = t.Name, slug = t.Slug, language = t.Language });
    }

    private static async Task<IResult> PatchTag(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.WebTags.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TagPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) t.Name = body.name;
            if (body.slug != null) t.Slug = body.slug;
            if (body.language != null) t.Language = body.language;
            t.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = t.Id, name = t.Name, slug = t.Slug, language = t.Language });
    }

    private static async Task<IResult> DeleteTag(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.WebTags.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        db.WebTags.Remove(t);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Web Categories ────────────────────────────────────────────────────────

    private static async Task<IResult> ListWebCategories(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebCategories.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderBy(c => c.Name).Select(c => new { id = c.Id, name = c.Name, slug = c.Slug }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateWebCategory(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<WebCategoryCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var c = new WebCategory
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Slug = body.slug ?? "",
            Description = body.description ?? "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebCategories.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/categories/", new { id = c.Id, name = c.Name, slug = c.Slug });
    }

    private static async Task<IResult> GetWebCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.WebCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, name = c.Name, slug = c.Slug, description = c.Description });
    }

    private static async Task<IResult> PatchWebCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.WebCategories.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<WebCategoryPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) c.Name = body.name;
            if (body.slug != null) c.Slug = body.slug;
            if (body.description != null) c.Description = body.description;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, name = c.Name, slug = c.Slug, description = c.Description });
    }

    private static async Task<IResult> DeleteWebCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.WebCategories.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        db.WebCategories.Remove(c);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Newsletter ────────────────────────────────────────────────────────────

    private static async Task<IResult> ListNewsletterSubscriptions(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.NewsletterSubscriptions.AsNoTracking();
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(s => s.CreatedAt).Select(s => new { id = s.Id, email = s.Email, is_active = s.IsActive, created_at = s.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateNewsletterSubscription(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<NewsletterSubscriptionCreateBody>(ct);
        if (body == null || string.IsNullOrEmpty(body.email)) return Results.BadRequest();
        var existing = await db.NewsletterSubscriptions.FirstOrDefaultAsync(s => s.WorkspaceId == wid.Value && s.Email == body.email, ct);
        if (existing != null)
        {
            existing.IsActive = true;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { id = existing.Id, email = existing.Email, is_active = existing.IsActive });
        }
        var s = new NewsletterSubscription
        {
            WorkspaceId = wid.Value,
            Email = body.email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.NewsletterSubscriptions.Add(s);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/newsletter/subscriptions/", new { id = s.Id, email = s.Email, is_active = s.IsActive });
    }

    private static async Task<IResult> DeleteNewsletterSubscription(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.NewsletterSubscriptions.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        s.IsActive = false;
        s.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListNewsletterTemplates(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.NewsletterTemplates.AsNoTracking();
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.OrderBy(t => t.Name).Select(t => new { id = t.Id, name = t.Name, subject = t.Subject }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateNewsletterTemplate(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<NewsletterTemplateCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new NewsletterTemplate
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Subject = body.subject ?? "",
            Body = body.body ?? "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.NewsletterTemplates.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/newsletter/templates/", new { id = t.Id, name = t.Name, subject = t.Subject });
    }

    private static async Task<IResult> GetNewsletterTemplate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.NewsletterTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, name = t.Name, subject = t.Subject, body = t.Body });
    }

    private static async Task<IResult> PatchNewsletterTemplate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.NewsletterTemplates.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<NewsletterTemplatePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) t.Name = body.name;
            if (body.subject != null) t.Subject = body.subject;
            if (body.body != null) t.Body = body.body;
            t.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = t.Id, name = t.Name, subject = t.Subject });
    }

    private static async Task<IResult> ListNewsletterSends(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.NewsletterSends.AsNoTracking();
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(s => s.CreatedAt).Select(s => new { id = s.Id, template_id = s.TemplateId, status = s.Status, created_at = s.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateNewsletterSend(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<NewsletterSendCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var s = new NewsletterSend
        {
            WorkspaceId = wid.Value,
            TemplateId = body.template_id,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.NewsletterSends.Add(s);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/newsletter/sends/", new { id = s.Id, template_id = s.TemplateId, status = s.Status });
    }

    private static async Task<IResult> GetNewsletterSend(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.NewsletterSends.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        return Results.Ok(new { id = s.Id, template_id = s.TemplateId, status = s.Status, created_at = s.CreatedAt });
    }

    // ── Inquiries ─────────────────────────────────────────────────────────────

    private static async Task<IResult> ListInquiries(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebInquiries.AsNoTracking();
        if (wid.HasValue) query = query.Where(i => i.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(i => i.CreatedAt).Select(i => new { id = i.Id, name = i.Name, email = i.Email, subject = i.Subject, status = i.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetInquiry(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var i = await db.WebInquiries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (i == null) return Results.NotFound();
        return Results.Ok(new { id = i.Id, name = i.Name, email = i.Email, phone = i.Phone, subject = i.Subject, message = i.Message, status = i.Status, created_at = i.CreatedAt });
    }

    private static async Task<IResult> PatchInquiry(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var inq = await db.WebInquiries.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (inq == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<InquiryPatchBody>(ct);
        if (body != null)
        {
            if (body.status != null) inq.Status = body.status;
            inq.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = inq.Id, status = inq.Status });
    }

    private static async Task<IResult> DeleteInquiry(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var inq = await db.WebInquiries.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (inq == null) return Results.NotFound();
        db.WebInquiries.Remove(inq);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetInquiryStats(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.WebInquiries.AsNoTracking();
        if (wid.HasValue) query = query.Where(i => i.WorkspaceId == wid.Value);
        var stats = await query.GroupBy(i => i.Status).Select(g => new { status = g.Key, count = g.Count() }).ToListAsync(ct);
        return Results.Ok(stats);
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    private static async Task<IResult> ListBookings(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Bookings.AsNoTracking();
        if (wid.HasValue) query = query.Where(b => b.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(b => b.CreatedAt).Select(b => new { id = b.Id, status = b.Status, created_at = b.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateBooking(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<BookingCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var b = new Booking
        {
            WorkspaceId = wid.Value,
            Name = body.customer_name ?? "",
            Email = body.customer_email ?? "",
            Phone = body.customer_phone ?? "",
            Notes = body.notes ?? "",
            Status = "pending",
            TimeslotId = body.time_slot_id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Bookings.Add(b);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/bookings/", new { id = b.Id, status = b.Status, customer_name = b.Name, customer_email = b.Email });
    }

    private static async Task<IResult> GetBooking(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Bookings.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        return Results.Ok(new { id = b.Id, status = b.Status, customer_name = b.Name, customer_email = b.Email, customer_phone = b.Phone, notes = b.Notes, time_slot_id = b.TimeslotId, created_at = b.CreatedAt });
    }

    private static async Task<IResult> PatchBooking(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<BookingPatchBody>(ct);
        if (body != null)
        {
            if (body.customer_name != null) b.Name = body.customer_name;
            if (body.customer_email != null) b.Email = body.customer_email;
            if (body.customer_phone != null) b.Phone = body.customer_phone;
            if (body.notes != null) b.Notes = body.notes;
            if (body.status != null) b.Status = body.status;
            b.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = b.Id, status = b.Status, customer_name = b.Name });
    }

    private static async Task<IResult> CancelBooking(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        b.Status = "cancelled";
        b.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = b.Id, status = b.Status });
    }

    private static async Task<IResult> ConfirmBooking(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Bookings.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        b.Status = "confirmed";
        b.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = b.Id, status = b.Status });
    }

    // ── Time Slots ────────────────────────────────────────────────────────────

    private static async Task<IResult> ListTimeSlots(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.BookingTimeSlots.AsNoTracking();
        if (wid.HasValue) query = query.Where(ts => ts.WorkspaceId == wid.Value);
        var list = await query.OrderBy(ts => ts.Date).ThenBy(ts => ts.StartTime).Select(ts => new { id = ts.Id, start_time = ts.StartTime, end_time = ts.EndTime, capacity = ts.MaxBookings, is_available = ts.IsActive && ts.CurrentBookings < ts.MaxBookings }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTimeSlot(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TimeSlotCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var ts = new BookingTimeSlot
        {
            WorkspaceId = wid.Value,
            Date = body.start_time.Date == DateTime.MinValue.Date ? DateTime.UtcNow.Date : body.start_time.Date,
            StartTime = TimeOnly.FromDateTime(body.start_time),
            EndTime = TimeOnly.FromDateTime(body.end_time),
            MaxBookings = body.capacity ?? 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.BookingTimeSlots.Add(ts);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/timeslots/", new { id = ts.Id, start_time = ts.StartTime, end_time = ts.EndTime, capacity = ts.MaxBookings, is_available = ts.IsActive && ts.CurrentBookings < ts.MaxBookings });
    }

    private static async Task<IResult> GetTimeSlot(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var ts = await db.BookingTimeSlots.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (ts == null) return Results.NotFound();
        return Results.Ok(new { id = ts.Id, start_time = ts.StartTime, end_time = ts.EndTime, capacity = ts.MaxBookings, is_available = ts.IsActive && ts.CurrentBookings < ts.MaxBookings });
    }

    private static async Task<IResult> PatchTimeSlot(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var ts = await db.BookingTimeSlots.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (ts == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TimeSlotPatchBody>(ct);
        if (body != null)
        {
            if (body.start_time.HasValue) ts.StartTime = TimeOnly.FromDateTime(body.start_time.Value);
            if (body.end_time.HasValue) ts.EndTime = TimeOnly.FromDateTime(body.end_time.Value);
            if (body.capacity.HasValue) ts.MaxBookings = body.capacity.Value;
            if (body.is_available.HasValue) ts.IsActive = body.is_available.Value;
            ts.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = ts.Id, start_time = ts.StartTime, end_time = ts.EndTime, capacity = ts.MaxBookings, is_available = ts.IsActive && ts.CurrentBookings < ts.MaxBookings });
    }

    private static async Task<IResult> GetAvailableTimeSlots(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.BookingTimeSlots.AsNoTracking().Where(ts => ts.IsActive && ts.CurrentBookings < ts.MaxBookings);
        if (wid.HasValue) query = query.Where(ts => ts.WorkspaceId == wid.Value);
        if (req.Query.TryGetValue("date", out var dateVal) && DateOnly.TryParse(dateVal, out var date))
        {
            var dayStart = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);
            query = query.Where(ts => ts.Date >= dayStart && ts.Date < dayEnd);
        }
        var list = await query.OrderBy(ts => ts.Date).ThenBy(ts => ts.StartTime).Select(ts => new { id = ts.Id, start_time = ts.StartTime, end_time = ts.EndTime, capacity = ts.MaxBookings }).ToListAsync(ct);
        return Results.Ok(list);
    }

    // ── Feedback ──────────────────────────────────────────────────────────────

    private static async Task<IResult> SubmitFeedback(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<FeedbackCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var inq = new Inquiry
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Email = body.email ?? "",
            Phone = body.phone ?? "",
            Subject = "feedback",
            Message = body.message ?? "",
            Status = "new",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.WebInquiries.Add(inq);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/feedback/", new { id = inq.Id, subject = inq.Subject, status = inq.Status });
    }

    // ── Media / Blocks / Newsletter helpers (existing) ───────────────────────

    private static async Task<IResult> UploadMedia(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();

        string fileName, fileUrl, responseFileType, responseMimeType;
        // Support both JSON and multipart
        if (ctx.Request.ContentType != null && ctx.Request.ContentType.Contains("multipart"))
        {
            var file = ctx.Request.Form.Files.GetFile("file");
            fileName = ctx.Request.Form["file_name"].ToString() ?? file?.FileName ?? "upload";
            var ft = ctx.Request.Form["file_type"].ToString();
            var mt = ctx.Request.Form["mime_type"].ToString();
            responseFileType = !string.IsNullOrEmpty(ft) ? ft : (!string.IsNullOrEmpty(mt) ? mt : "image");
            responseMimeType = !string.IsNullOrEmpty(mt) ? mt : responseFileType;
            fileUrl = ctx.Request.Form["file_url"].ToString() ?? "";
        }
        else
        {
            var body = await ctx.Request.ReadFromJsonAsync<MediaCreateBody>(ct);
            fileName = body?.file_name ?? "upload";
            responseFileType = body?.file_type ?? body?.mime_type ?? "image";
            responseMimeType = body?.mime_type ?? responseFileType;
            fileUrl = body?.file_url ?? "";
        }

        var media = new Media
        {
            WorkspaceId = wid.Value,
            File = fileUrl.Length > 0 ? fileUrl : fileName,
            MediaType = responseFileType,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Media.Add(media);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/web/media/", new { id = media.Id, file_name = fileName, file_url = fileUrl, file_type = responseFileType, mime_type = responseMimeType });
    }

    private static IResult BlockTypes() => Results.Ok(new object[] { });

    private static IResult BlockValidate() => Results.Ok(new { valid = true });

    private static IResult NewsletterUnsubscribe() => Results.Ok(new { detail = "Unsubscribed." });

    // ── Request body records ──────────────────────────────────────────────────

    private sealed record PageTreeNode(int Id, string Title, string Slug, string Status, int? ParentId);
    private sealed record MediaCreateBody(string? file_name, string? file_url, string? mime_type, string? file_type);
    private sealed record SiteCreateBody(string? name, string? domain, string? site_title, string? default_language);
    private sealed record SitePatchBody(string? name, string? domain, bool? is_active, bool? is_default);
    private sealed record ThemeCreateBody(string? name, string? code);
    private sealed record ThemePatchBody(string? name, string? code, bool? is_active);
    private sealed record LanguageCreateBody(string? code, string? name, bool? is_default, int? sort_order);
    private sealed record LanguagePatchBody(string? name, bool? is_default, int? sort_order);
    private sealed record PageCreateBody(string? title, string? slug, string? content, string? status, string? language);
    private sealed record PagePatchBody(string? title, string? slug, string? content, string? status, string? language);
    private sealed record PostCreateBody(string? title, string? slug, string? content, string? excerpt, string? status, string? language);
    private sealed record PostPatchBody(string? title, string? slug, string? content, string? excerpt, string? status, string? language);
    private sealed record MenuCreateBody(string? name, string? slug, string? location, string? language);
    private sealed record MenuPatchBody(string? name, string? slug, string? location, string? language);
    private sealed record MenuItemCreateBody(int menu_id, string? label, string? url, int? sort_order, int? parent_id);
    private sealed record MenuItemPatchBody(string? label, string? url, int? sort_order, int? parent_id);
    private sealed record TagCreateBody(string? name, string? slug, string? language);
    private sealed record TagPatchBody(string? name, string? slug, string? language);
    private sealed record WebCategoryCreateBody(string? name, string? slug, string? description);
    private sealed record WebCategoryPatchBody(string? name, string? slug, string? description);
    private sealed record NewsletterSubscriptionCreateBody(string? email);
    private sealed record NewsletterTemplateCreateBody(string? name, string? subject, string? body);
    private sealed record NewsletterTemplatePatchBody(string? name, string? subject, string? body);
    private sealed record NewsletterSendCreateBody(int template_id);
    private sealed record InquiryPatchBody(string? status);
    private sealed record BookingCreateBody(string? customer_name, string? customer_email, string? customer_phone, string? notes, int? time_slot_id);
    private sealed record BookingPatchBody(string? customer_name, string? customer_email, string? customer_phone, string? notes, string? status);
    private sealed record TimeSlotCreateBody(DateTime start_time, DateTime end_time, int? capacity);
    private sealed record TimeSlotPatchBody(DateTime? start_time, DateTime? end_time, int? capacity, bool? is_available);
    private sealed record FeedbackCreateBody(string? name, string? email, string? phone, string? message);
}
