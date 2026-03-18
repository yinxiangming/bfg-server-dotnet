namespace Bfg.Core.Inbox;

/// <summary>
/// Message/notification. Matches Django inbox.Message.
/// </summary>
public class InboxMessage
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public string MessageType { get; set; } = "notification";
    public int? SenderId { get; set; }
    public bool SendEmail { get; set; }
    public bool SendSms { get; set; }
    public bool SendPush { get; set; }
    public DateTime CreatedAt { get; set; }
}
