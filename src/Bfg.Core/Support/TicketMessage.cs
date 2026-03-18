namespace Bfg.Core.Support;

/// <summary>
/// Ticket reply/message. Matches Django support.TicketMessage.
/// </summary>
public class TicketMessage
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int? UserId { get; set; }
    public string Body { get; set; } = "";
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }

    public SupportTicket Ticket { get; set; } = null!;
}
