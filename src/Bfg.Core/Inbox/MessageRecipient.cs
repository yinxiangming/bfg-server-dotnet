namespace Bfg.Core.Inbox;

public class MessageRecipient
{
    public int Id { get; set; }
    public int MessageId { get; set; }
    public int RecipientId { get; set; }
    public bool IsRead { get; set; }
    public bool IsArchived { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
}
