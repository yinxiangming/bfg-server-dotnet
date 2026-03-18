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

        group.MapGet("/tickets/{id:int}/messages", ListTicketMessages);
        group.MapPost("/tickets/{id:int}/messages/", CreateTicketMessage);
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
        var t = new SupportTicket
        {
            WorkspaceId = wid.Value,
            CustomerId = body.customer > 0 ? body.customer : null,
            Subject = body.subject ?? "",
            Description = body.description ?? "",
            Status = body.status ?? "new",
            Channel = body.channel ?? "web",
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
        var m = new TicketMessage { TicketId = id, UserId = userId, Body = body.message ?? "", IsInternal = body.is_internal ?? false, CreatedAt = DateTime.UtcNow };
        db.TicketMessages.Add(m);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/support/tickets/{id}/messages/", new { id = m.Id, body = m.Body });
    }

    private sealed record TicketCreateBody(string? subject, string? description, int customer, string? status, string? channel);
    private sealed record TicketPatchBody(string? status, string? description);
    private sealed record TicketMessageCreateBody(string? message, bool? is_internal);
}
