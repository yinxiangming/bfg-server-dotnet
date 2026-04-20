namespace Bfg.Core.Promo;

public class StampRecord
{
    public int Id { get; set; }
    public int CampaignId { get; set; }
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public int StampCount { get; set; } = 1;
    public DateTime CreatedAt { get; set; }
    public int? CreatedById { get; set; }
}
