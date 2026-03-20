namespace Bfg.Core.Promo;

/// <summary>
/// Discount rule. Matches Django marketing.DiscountRule.
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
    public string Config { get; set; } = "{}";
    public int? PrerequisiteQuantity { get; set; }
    public string PrerequisiteProductIds { get; set; } = "[]";
    public int? EntitledQuantity { get; set; }
    public string EntitledProductIds { get; set; } = "[]";
    public string AllocationMethod { get; set; } = "across";
    public string DisplayLabel { get; set; } = "";
    public int? PromoDisplayOrder { get; set; }
    public bool IsGroupBuy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
}
