using System.Text.Json.Serialization;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Inbox;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class InboxEndpoints
{
    public static void MapInboxEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inbox").WithTags("Inbox").RequireAuthorization();
        group.MapGet("/messages", ListMessages);
        group.MapPost("/messages/", CreateMessage);
        group.MapPost("/messages/{id:int}/send/", SendMessage);
        group.MapGet("/templates", ListTemplates);
        group.MapPost("/templates/", CreateTemplate);
        group.MapGet("/templates/{id:int}", GetTemplate);
        group.MapPatch("/templates/{id:int}", PatchTemplate);
        group.MapGet("/message-templates", ListTemplates);
        group.MapPost("/message-templates/", CreateTemplate);
        group.MapGet("/message-templates/{id:int}", GetTemplate);
        group.MapPatch("/message-templates/{id:int}", PatchTemplate);
        group.MapGet("/sms", EmptyList);

        // Message Recipients
        group.MapGet("/recipients", ListRecipients);
        group.MapPost("/recipients/", CreateRecipient);
        group.MapGet("/recipients/unread_count", GetUnreadCount);
        group.MapPost("/recipients/bulk_mark_read", BulkMarkRead);
        group.MapPost("/recipients/bulk_mark_unread", BulkMarkUnread);
        group.MapPost("/recipients/bulk_delete", BulkDelete);
        group.MapPost("/recipients/mark_all_read", MarkAllRead);
        group.MapGet("/recipients/{id:int}", GetRecipient);
        group.MapDelete("/recipients/{id:int}", DeleteRecipient);
        group.MapPost("/recipients/{id:int}/archive", ArchiveRecipient);
        group.MapPost("/recipients/{id:int}/mark_read", MarkRecipientRead);
        group.MapPost("/recipients/{id:int}/mark_unread", MarkRecipientUnread);

        // Notifications
        group.MapGet("/notifications", ListNotifications);
        group.MapGet("/notifications/{id:int}", GetNotification);
        group.MapPost("/notifications/{id:int}/mark_read", MarkNotificationRead);
    }

    private static async Task<IResult> ListMessages(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.InboxMessages.AsNoTracking().Where(m => !wid.HasValue || m.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(m => m.CreatedAt)
            .Select(m => new { id = m.Id, subject = m.Subject, message = m.Message, message_type = m.MessageType, created_at = m.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateMessage(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<MessageCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var msgType = body.message_type ?? "notification";
        if (msgType.Length > 20) msgType = msgType[..20];
        var m = new InboxMessage
        {
            WorkspaceId = wid.Value,
            Subject = body.subject ?? "",
            Message = body.message ?? "",
            MessageType = msgType,
            ActionUrl = "",
            ActionLabel = "",
            SendEmail = body.send_email ?? false,
            SendSms = body.send_sms ?? false,
            SendPush = body.send_push ?? false,
            CreatedAt = DateTime.UtcNow
        };
        db.InboxMessages.Add(m);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/inbox/messages/", new { id = m.Id, subject = m.Subject, message_type = m.MessageType });
    }

    private static async Task<IResult> SendMessage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.InboxMessages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        return Results.Ok(new { success = true });
    }

    private static async Task<IResult> ListTemplates(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.MessageTemplates.AsNoTracking()
            .Where(t => !wid.HasValue || t.WorkspaceId == null || t.WorkspaceId == wid.Value);
        var list = await query.Select(t => new { id = t.Id, name = t.Name, code = t.Code, event_type = t.Event, language = t.Language, is_active = t.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateTemplate(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var body = await ctx.Request.ReadFromJsonAsync<TemplateCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new MessageTemplate
        {
            WorkspaceId = wid,
            Name = body.name ?? "",
            Code = body.code ?? "",
            Event = body.@event ?? "",
            Language = body.language ?? "en",
            EmailEnabled = body.email_enabled ?? false,
            EmailSubject = body.email_subject ?? "",
            EmailBody = body.email_body ?? "",
            EmailHtmlBody = "",
            AppMessageEnabled = body.app_message_enabled ?? false,
            AppMessageTitle = body.app_message_title ?? "",
            AppMessageBody = body.app_message_body ?? "",
            SmsEnabled = false,
            SmsBody = "",
            PushEnabled = false,
            PushTitle = "",
            PushBody = "",
            AvailableVariables = "[]",
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.MessageTemplates.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/inbox/templates/", new { id = t.Id, name = t.Name, code = t.Code, Event = t.Event });
    }

    private static async Task<IResult> GetTemplate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.MessageTemplates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == null || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, name = t.Name, code = t.Code, event_type = t.Event, app_message_body = t.AppMessageBody });
    }

    private static async Task<IResult> PatchTemplate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.MessageTemplates.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == null || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TemplatePatchBody>(ct);
        if (body?.app_message_body != null) { t.AppMessageBody = body.app_message_body; t.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
        return Results.Ok(new { id = t.Id, app_message_body = t.AppMessageBody });
    }

    private static IResult EmptyList() => Results.Ok(Array.Empty<object>());

    // --- Message Recipients ---

    private static async Task<IResult> ListRecipients(BfgDbContext db, HttpContext ctx, int? message_id, int page, int page_size, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var query = db.MessageRecipients.AsNoTracking().Where(r => r.RecipientId == userId.Value);
        if (message_id.HasValue) query = query.Where(r => r.MessageId == message_id.Value);
        var total = await query.CountAsync(ct);
        if (page < 1) page = 1;
        if (page_size < 1) page_size = 20;
        var list = await query.OrderByDescending(r => r.Id).Skip((page - 1) * page_size).Take(page_size)
            .Select(r => new { id = r.Id, message_id = r.MessageId, recipient_id = r.RecipientId, is_read = r.IsRead, is_archived = r.IsArchived, is_deleted = r.IsDeleted, read_at = r.ReadAt, delivered_at = r.DeliveredAt })
            .ToListAsync(ct);
        return Results.Ok(new { count = total, results = list });
    }

    private static async Task<IResult> CreateRecipient(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<RecipientCreateBody>(ct);
        if (body == null || body.message_id <= 0 || body.recipient_id <= 0) return Results.BadRequest();
        var r = new MessageRecipient
        {
            MessageId = body.message_id,
            RecipientId = body.recipient_id,
            IsRead = false,
            IsArchived = false,
            IsDeleted = false,
            DeliveredAt = DateTime.UtcNow
        };
        db.MessageRecipients.Add(r);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/inbox/recipients/", new { id = r.Id, message_id = r.MessageId, recipient_id = r.RecipientId });
    }

    private static async Task<IResult> GetRecipient(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var r = await db.MessageRecipients.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == userId.Value, ct);
        if (r == null) return Results.NotFound();
        return Results.Ok(new { id = r.Id, message_id = r.MessageId, recipient_id = r.RecipientId, is_read = r.IsRead, is_archived = r.IsArchived, is_deleted = r.IsDeleted, read_at = r.ReadAt });
    }

    private static async Task<IResult> DeleteRecipient(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var r = await db.MessageRecipients.FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == userId.Value, ct);
        if (r == null) return Results.NotFound();
        r.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> BulkMarkRead(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<BulkIdsBody>(ct);
        if (body?.ids == null) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var items = await db.MessageRecipients.Where(r => body.ids.Contains(r.Id) && r.RecipientId == userId.Value).ToListAsync(ct);
        foreach (var r in items) { r.IsRead = true; r.ReadAt = now; }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { updated = items.Count });
    }

    private static async Task<IResult> BulkMarkUnread(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<BulkIdsBody>(ct);
        if (body?.ids == null) return Results.BadRequest();
        var items = await db.MessageRecipients.Where(r => body.ids.Contains(r.Id) && r.RecipientId == userId.Value).ToListAsync(ct);
        foreach (var r in items) { r.IsRead = false; r.ReadAt = null; }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { updated = items.Count });
    }

    private static async Task<IResult> BulkDelete(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<BulkIdsBody>(ct);
        if (body?.ids == null) return Results.BadRequest();
        var items = await db.MessageRecipients.Where(r => body.ids.Contains(r.Id) && r.RecipientId == userId.Value).ToListAsync(ct);
        foreach (var r in items) r.IsDeleted = true;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { updated = items.Count });
    }

    private static async Task<IResult> MarkAllRead(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var now = DateTime.UtcNow;
        var items = await db.MessageRecipients.Where(r => r.RecipientId == userId.Value && !r.IsRead && !r.IsDeleted).ToListAsync(ct);
        foreach (var r in items) { r.IsRead = true; r.ReadAt = now; }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { updated = items.Count });
    }

    private static async Task<IResult> GetUnreadCount(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var count = await db.MessageRecipients.AsNoTracking().CountAsync(r => r.RecipientId == userId.Value && !r.IsRead && !r.IsDeleted, ct);
        return Results.Ok(new { unread_count = count });
    }

    private static async Task<IResult> ArchiveRecipient(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var r = await db.MessageRecipients.FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == userId.Value, ct);
        if (r == null) return Results.NotFound();
        r.IsArchived = true;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = r.Id, is_archived = r.IsArchived });
    }

    private static async Task<IResult> MarkRecipientRead(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var r = await db.MessageRecipients.FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == userId.Value, ct);
        if (r == null) return Results.NotFound();
        r.IsRead = true;
        r.ReadAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = r.Id, is_read = r.IsRead, read_at = r.ReadAt });
    }

    private static async Task<IResult> MarkRecipientUnread(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var r = await db.MessageRecipients.FirstOrDefaultAsync(x => x.Id == id && x.RecipientId == userId.Value, ct);
        if (r == null) return Results.NotFound();
        r.IsRead = false;
        r.ReadAt = null;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = r.Id, is_read = r.IsRead });
    }

    // --- Notifications ---

    private static async Task<IResult> ListNotifications(BfgDbContext db, HttpContext ctx, int page, int page_size, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        if (page < 1) page = 1;
        if (page_size < 1) page_size = 20;
        var query = db.Notifications.AsNoTracking().Where(n => n.UserId == userId.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending(n => n.CreatedAt).Skip((page - 1) * page_size).Take(page_size)
            .Select(n => new { id = n.Id, title = n.Title, body = n.Body, is_read = n.IsRead, created_at = n.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(new { count = total, results = list });
    }

    private static async Task<IResult> GetNotification(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var n = await db.Notifications.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId.Value, ct);
        if (n == null) return Results.NotFound();
        return Results.Ok(new { id = n.Id, title = n.Title, body = n.Body, is_read = n.IsRead, created_at = n.CreatedAt });
    }

    private static async Task<IResult> MarkNotificationRead(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        int? userId = int.TryParse(userIdClaim, out var uid) ? uid : null;
        if (!userId.HasValue) return Results.Unauthorized();
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId.Value, ct);
        if (n == null) return Results.NotFound();
        n.IsRead = true;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = n.Id, is_read = n.IsRead });
    }

    private sealed record MessageCreateBody(string? subject, string? message, string? message_type, bool? send_email, bool? send_sms, bool? send_push);
    private sealed record TemplateCreateBody(string? name, string? code, string? @event, string? language, bool? email_enabled, string? email_subject, string? email_body, bool? app_message_enabled, string? app_message_title, string? app_message_body, bool? is_active);
    private sealed record TemplatePatchBody(string? app_message_body);
    private sealed record RecipientCreateBody(int message_id, int recipient_id);
    private sealed record BulkIdsBody(List<int>? ids);
}
