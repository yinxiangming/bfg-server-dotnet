namespace Bfg.Core.Finance;

public class WithdrawalRequest
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public int? TransactionId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "pending";
    public string PayoutMethod { get; set; } = "";
    public string PayoutDetails { get; set; } = "";
    public string Notes { get; set; } = "";
    public string RejectionReason { get; set; } = "";
    public DateTime RequestedAt { get; set; }
    public int? RequestedById { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
