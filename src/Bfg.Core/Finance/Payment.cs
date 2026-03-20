namespace Bfg.Core.Finance;

/// <summary>
/// Payment record. Matches Django resale MySQL table finance_payment (not legacy Prisma shape).
/// </summary>
public class Payment
{
    public long Id { get; set; }
    public long WorkspaceId { get; set; }
    public long? OrderId { get; set; }
    public long CustomerId { get; set; }
    public long? GatewayId { get; set; }
    public long CurrencyId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public string PaymentNumber { get; set; } = "";
    public string GatewayDisplayName { get; set; } = "";
    public string GatewayType { get; set; } = "custom";
    public string GatewayTransactionId { get; set; } = "";
    public string GatewayResponse { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long? InvoiceId { get; set; }
    public long? PaymentMethodId { get; set; }
}
