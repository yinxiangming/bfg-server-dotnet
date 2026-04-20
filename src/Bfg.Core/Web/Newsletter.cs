namespace Bfg.Core.Web;

public class NewsletterSubscription
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? SiteId { get; set; }
    public string Email { get; set; } = "";
    public string Status { get; set; } = "active";
    public bool IsActive { get; set; } = true;
    public string SourceUrl { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string UnsubscribeToken { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NewsletterTemplate
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Subject { get; set; } = "";
    public string SubjectTemplate { get; set; } = "";
    public string Body { get; set; } = "";
    public string BodyHtml { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class NewsletterSend
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Subject { get; set; } = "";
    public string Content { get; set; } = "";
    public int? TemplateId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string Status { get; set; } = "draft";
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CreatedById { get; set; }
}
