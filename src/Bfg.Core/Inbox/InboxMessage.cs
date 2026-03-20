namespace Bfg.Core.Inbox;

/// <summary>
/// In-app message. Matches Django inbox.Message (MySQL inbox_message).
/// </summary>
public class InboxMessage
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public string MessageType { get; set; } = "notification";
    public string ActionUrl { get; set; } = "";
    public string ActionLabel { get; set; } = "";
    public int? RelatedObjectId { get; set; }
    public int? RelatedContentTypeId { get; set; }
    public int? SenderId { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
    public bool SendPush { get; set; }
    public DateTime? SendEmailAt { get; set; }
    public DateTime? SendSmsAt { get; set; }
    public DateTime? SendPushAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
