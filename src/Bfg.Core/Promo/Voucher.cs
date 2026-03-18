namespace Bfg.Core.Promo;

/// <summary>
/// Discount voucher/coupon. Matches Django promo.Voucher / marketing.Coupon concept.
/// </summary>
public class Voucher
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? DiscountRuleId { get; set; }
    public int? CampaignId { get; set; }
    public string Code { get; set; } = "";
    public string? Description { get; set; }
    public string DiscountType { get; set; } = "percentage";
    public decimal DiscountValue { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UsageLimit { get; set; }
    public int UsedCount { get; set; }
    public int? UsageLimitPerCustomer { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
