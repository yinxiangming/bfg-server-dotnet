namespace Bfg.Core.Support;

/// <summary>
/// Support ticket. Matches Django support.SupportTicket.
/// </summary>
public class SupportTicket
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? CustomerId { get; set; }
    public string Subject { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "new";
    public string? Channel { get; set; }
    public string? Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
