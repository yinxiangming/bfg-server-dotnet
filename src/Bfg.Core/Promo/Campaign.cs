namespace Bfg.Core.Promo;

/// <summary>
/// Marketing campaign. Matches Django marketing.Campaign.
/// </summary>
public class Campaign
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string CampaignType { get; set; } = "email";
    public string Description { get; set; } = "";
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Budget { get; set; }
    public string UtmSource { get; set; } = "";
    public string UtmMedium { get; set; } = "";
    public string UtmCampaign { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool RequiresParticipation { get; set; }
    public int? MinParticipants { get; set; }
    public int? MaxParticipants { get; set; }
    public string PromoDisplayType { get; set; } = "";
    public string Config { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
