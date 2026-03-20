namespace Bfg.Core.Support;

/// <summary>
/// Ticket reply. Django columns: message, sender_id, is_staff_reply.
/// </summary>
public class TicketMessage
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int? UserId { get; set; }
    public string Body { get; set; } = "";
    public bool IsStaffReply { get; set; }
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }

    public SupportTicket Ticket { get; set; } = null!;
}
