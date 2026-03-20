namespace Bfg.Core.Support;

/// <summary>
/// Support ticket. Matches Django support.SupportTicket (priority is FK priority_id, not a string column).
/// </summary>
public class SupportTicket
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int CustomerId { get; set; }
    public string TicketNumber { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Description { get; set; } = "";
    public string Status { get; set; } = "new";
    public string Channel { get; set; } = "web";
    public int? CategoryId { get; set; }
    public int? PriorityId { get; set; }
    public int? RelatedOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
