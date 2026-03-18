namespace Bfg.Core.Promo;

/// <summary>
/// Discount rule for marketing. Matches Django marketing.DiscountRule.
/// </summary>
public class DiscountRule
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public string ApplyTo { get; set; } = "order";
    public decimal? MaximumDiscount { get; set; }
    public decimal? MinimumPurchase { get; set; }
    public string? DisplayLabel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
