namespace Bfg.Core.Finance;

public class Transaction
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? CustomerId { get; set; }
    public string TransactionType { get; set; } = "";
    public decimal Amount { get; set; }
    public int? CurrencyId { get; set; }
    public int? WalletId { get; set; }
    public string BalanceType { get; set; } = "cash";
    public string TxStatus { get; set; } = "completed";
    public string SourceType { get; set; } = "";
    public int? SourceId { get; set; }
    public int? PaymentId { get; set; }
    public int? InvoiceId { get; set; }
    public string Description { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int? CreatedById { get; set; }
}
