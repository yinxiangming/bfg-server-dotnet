namespace Bfg.Core.Promo;

/// <summary>
/// Gift card. Matches Django marketing.GiftCard.
/// </summary>
public class GiftCard
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? CustomerId { get; set; }
    public int CurrencyId { get; set; }
    public decimal InitialValue { get; set; }
    public decimal Balance { get; set; }
    public string Code { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
