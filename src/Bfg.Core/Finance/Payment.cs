namespace Bfg.Core.Finance;

/// <summary>
/// Payment record. Matches Django finance.Payment.
/// </summary>
public class Payment
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? OrderId { get; set; }
    public int? CustomerId { get; set; }
    public int? GatewayId { get; set; }
    public int? CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public string? TransactionId { get; set; }
    public string? PaymentNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
