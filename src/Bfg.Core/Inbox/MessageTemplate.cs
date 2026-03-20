namespace Bfg.Core.Inbox;

/// <summary>
/// Message template. Matches Django inbox.MessageTemplate.
/// </summary>
public class MessageTemplate
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Event { get; set; } = "";
    public string Language { get; set; } = "en";
    public bool EmailEnabled { get; set; }
    public string EmailSubject { get; set; } = "";
    public string EmailBody { get; set; } = "";
    public string EmailHtmlBody { get; set; } = "";
    public bool AppMessageEnabled { get; set; }
    public string AppMessageTitle { get; set; } = "";
    public string AppMessageBody { get; set; } = "";
    public bool SmsEnabled { get; set; }
    public string SmsBody { get; set; } = "";
    public bool PushEnabled { get; set; }
    public string PushTitle { get; set; } = "";
    public string PushBody { get; set; } = "";
    public string AvailableVariables { get; set; } = "[]";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
