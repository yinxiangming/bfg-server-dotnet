namespace Bfg.Core.Finance;

/// <summary>
/// Invoice. Column names aligned with Django finance.Invoice (total, issue_date, etc.).
/// </summary>
public class Invoice
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? OrderId { get; set; }
    public int CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = "";
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    /// <summary>DB column: total</summary>
    public decimal TotalAmount { get; set; }
    public int CurrencyId { get; set; }
    public string Status { get; set; } = "draft";
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
