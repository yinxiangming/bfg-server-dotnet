namespace Bfg.Core.Finance;

public class Wallet
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int CustomerId { get; set; }
    public decimal CashBalance { get; set; }
    public decimal CreditBalance { get; set; }
    public int CurrencyId { get; set; }
    public decimal CreditLimit { get; set; }
    public DateTime UpdatedAt { get; set; }
}
