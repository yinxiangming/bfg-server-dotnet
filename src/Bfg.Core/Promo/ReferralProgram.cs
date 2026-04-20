namespace Bfg.Core.Promo;

public class ReferralProgram
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string RewardType { get; set; } = "discount";
    public decimal RewardValue { get; set; }
    public decimal MinPurchase { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
