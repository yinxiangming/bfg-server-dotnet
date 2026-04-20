using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Support;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class SupportEndpoints
{
    public static void MapSupportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/support").WithTags("Support").RequireAuthorization();

        group.MapGet("/tickets", ListTickets);
        group.MapPost("/tickets/", CreateTicket);
        group.MapGet("/tickets/{id:int}", GetTicket);
        group.MapPatch("/tickets/{id:int}", PatchTicket);
        group.MapDelete("/tickets/{id:int}", DeleteTicket);

        group.MapGet("/tickets/{id:int}/messages", ListTicketMessages);
        group.MapPost("/tickets/{id:int}/messages/", CreateTicketMessage);

        group.MapGet("/ticket-categories", ListTicketCategories);
        group.MapPost("/ticket-categories/", CreateTicketCategory);
        group.MapGet("/ticket-categories/{id:int}", GetTicketCategory);
        group.MapPatch("/ticket-categories/{id:int}", PatchTicketCategory);
        group.MapDelete("/ticket-categories/{id:int}", DeleteTicketCategory);

        group.MapGet("/ticket-priorities", ListTicketPriorities);
        group.MapPost("/ticket-priorities/", CreateTicketPriority);
        group.MapGet("/ticket-priorities/{id:int}", GetTicketPriority);
        group.MapPatch("/ticket-priorities/{id:int}", PatchTicketPriority);
        group.MapDelete("/ticket-priorities/{id:int}", DeleteTicketPriority);

        group.MapGet("/options", GetSupportOptions);
    }

    private static async Task<IResult> ListTickets(BfgDbContext db, HttpContext ctx, string? status, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.SupportTickets.AsNoTracking().Where(t => !wid.HasValue || t.WorkspaceId == wid.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
        var list = await query.OrderByDescending(t => t.CreatedAt)
            .Select(t => new { id = t.Id, subject = t.Subject, description = t.Description, customer = t.CustomerId, status = t.Status, channel = t.Channel, created_at = t.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTicket(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TicketCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        if (body.customer <= 0) return Results.BadRequest(new { customer = new[] { "This field is required." } });
        var t = new SupportTicket
        {
            WorkspaceId = wid.Value,
            CustomerId = body.customer,
            TicketNumber = "TKT-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            Subject = body.subject ?? "",
            Description = body.description ?? "",
            Status = body.status ?? "new",
            Channel = string.IsNullOrEmpty(body.channel) ? "web" : body.channel!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.SupportTickets.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/support/tickets/", new { id = t.Id, subject = t.Subject, status = t.Status, customer = t.CustomerId });
    }

    private static async Task<IResult> GetTicket(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, subject = t.Subject, description = t.Description, status = t.Status, customer = t.CustomerId });
    }

    private static async Task<IResult> PatchTicket(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.SupportTickets.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TicketPatchBody>(ct);
        if (body != null)
        {
            if (body.status != null) t.Status = body.status;
            if (body.description != null) t.Description = body.description;
            t.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = t.Id, subject = t.Subject, description = t.Description, status = t.Status });
    }

    private static async Task<IResult> DeleteTicket(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.SupportTickets.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        t.Status = "closed";
        t.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListTicketMessages(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var ticket = await db.SupportTickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id && (!wid.HasValue || t.WorkspaceId == wid.Value), ct);
        if (ticket == null) return Results.NotFound();
        var list = await db.TicketMessages.AsNoTracking().Where(m => m.TicketId == id).OrderBy(m => m.CreatedAt)
            .Select(m => new { id = m.Id, body = m.Body, is_internal = m.IsInternal, created_at = m.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTicketMessage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var ticket = await db.SupportTickets.FirstOrDefaultAsync(t => t.Id == id && (!wid.HasValue || t.WorkspaceId == wid.Value), ct);
        if (ticket == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TicketMessageCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        var m = new TicketMessage
        {
            TicketId = id,
            UserId = userId,
            Body = body.message ?? "",
            IsStaffReply = false,
            IsInternal = body.is_internal ?? false,
            CreatedAt = DateTime.UtcNow
        };
        db.TicketMessages.Add(m);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/support/tickets/{id}/messages/", new { id = m.Id, body = m.Body });
    }

    // --- Ticket Categories ---

    private static async Task<IResult> ListTicketCategories(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.TicketCategories.AsNoTracking()
            .Where(c => !wid.HasValue || c.WorkspaceId == wid.Value)
            .OrderBy(c => c.SortOrder)
            .Select(c => new { id = c.Id, name = c.Name, description = c.Description, order = c.SortOrder, is_active = c.IsActive })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTicketCategory(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TicketCategoryCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var c = new TicketCategory
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Description = body.description ?? "",
            SortOrder = body.order ?? 0,
            IsActive = body.is_active ?? true
        };
        db.TicketCategories.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/support/ticket-categories/", new { id = c.Id, name = c.Name, description = c.Description, order = c.SortOrder, is_active = c.IsActive });
    }

    private static async Task<IResult> GetTicketCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.TicketCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, name = c.Name, description = c.Description, order = c.SortOrder, is_active = c.IsActive });
    }

    private static async Task<IResult> PatchTicketCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.TicketCategories.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TicketCategoryPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) c.Name = body.name;
            if (body.description != null) c.Description = body.description;
            if (body.order.HasValue) c.SortOrder = body.order.Value;
            if (body.is_active.HasValue) c.IsActive = body.is_active.Value;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, name = c.Name, description = c.Description, order = c.SortOrder, is_active = c.IsActive });
    }

    private static async Task<IResult> DeleteTicketCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.TicketCategories.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        c.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // --- Ticket Priorities ---

    private static async Task<IResult> ListTicketPriorities(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.TicketPriorities.AsNoTracking()
            .Where(p => !wid.HasValue || p.WorkspaceId == wid.Value)
            .OrderBy(p => p.Level)
            .Select(p => new { id = p.Id, name = p.Name, level = p.Level, color = p.Color, response_time_hours = p.ResponseTimeHours, resolution_time_hours = p.ResolutionTimeHours, is_active = p.IsActive })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTicketPriority(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TicketPriorityCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var p = new TicketPriority
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Level = body.level ?? 1,
            Color = body.color ?? "#000000",
            ResponseTimeHours = body.response_time_hours ?? 24,
            ResolutionTimeHours = body.resolution_time_hours ?? 72,
            IsActive = true
        };
        db.TicketPriorities.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/support/ticket-priorities/", new { id = p.Id, name = p.Name, level = p.Level, color = p.Color, response_time_hours = p.ResponseTimeHours, resolution_time_hours = p.ResolutionTimeHours, is_active = p.IsActive });
    }

    private static async Task<IResult> GetTicketPriority(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.TicketPriorities.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { id = p.Id, name = p.Name, level = p.Level, color = p.Color, response_time_hours = p.ResponseTimeHours, resolution_time_hours = p.ResolutionTimeHours, is_active = p.IsActive });
    }

    private static async Task<IResult> PatchTicketPriority(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.TicketPriorities.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TicketPriorityPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) p.Name = body.name;
            if (body.level.HasValue) p.Level = body.level.Value;
            if (body.color != null) p.Color = body.color;
            if (body.response_time_hours.HasValue) p.ResponseTimeHours = body.response_time_hours.Value;
            if (body.resolution_time_hours.HasValue) p.ResolutionTimeHours = body.resolution_time_hours.Value;
            if (body.is_active.HasValue) p.IsActive = body.is_active.Value;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = p.Id, name = p.Name, level = p.Level, color = p.Color, response_time_hours = p.ResponseTimeHours, resolution_time_hours = p.ResolutionTimeHours, is_active = p.IsActive });
    }

    private static async Task<IResult> DeleteTicketPriority(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.TicketPriorities.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        p.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // --- Support Options ---

    private static IResult GetSupportOptions()
    {
        var options = new[]
        {
            new { type = "chat", available = true },
            new { type = "email", available = true }
        };
        return Results.Ok(options);
    }

    private sealed record TicketCreateBody(string? subject, string? description, int customer, string? status, string? channel);
    private sealed record TicketPatchBody(string? status, string? description);
    private sealed record TicketMessageCreateBody(string? message, bool? is_internal);
    private sealed record TicketCategoryCreateBody(string? name, string? description, int? order, bool? is_active);
    private sealed record TicketCategoryPatchBody(string? name, string? description, int? order, bool? is_active);
    private sealed record TicketPriorityCreateBody(string? name, int? level, string? color, int? response_time_hours, int? resolution_time_hours);
    private sealed record TicketPriorityPatchBody(string? name, int? level, string? color, int? response_time_hours, int? resolution_time_hours, bool? is_active);
}
