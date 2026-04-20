namespace Bfg.Core.Promo;

public class CampaignParticipation
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int CustomerId { get; set; }
    public string Status { get; set; } = "active";
    public int StampCount { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RewardClaimedAt { get; set; }
}
