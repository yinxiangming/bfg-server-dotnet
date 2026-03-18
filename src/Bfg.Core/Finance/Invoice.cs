namespace Bfg.Core.Finance;

/// <summary>
/// Invoice. Matches Django finance.Invoice.
/// </summary>
public class Invoice
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "draft";
    public DateTime? IssuedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
