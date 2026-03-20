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
        group.MapGet("/recipients", EmptyList);
        group.MapGet("/sms", EmptyList);
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

    private sealed record MessageCreateBody(string? subject, string? message, string? message_type, bool? send_email, bool? send_sms, bool? send_push);
    private sealed record TemplateCreateBody(string? name, string? code, string? @event, string? language, bool? email_enabled, string? email_subject, string? email_body, bool? app_message_enabled, string? app_message_title, string? app_message_body, bool? is_active);
    private sealed record TemplatePatchBody(string? app_message_body);
}
