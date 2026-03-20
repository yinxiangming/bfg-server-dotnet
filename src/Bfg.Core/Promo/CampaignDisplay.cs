namespace Bfg.Core.Promo;

/// <summary>
/// Storefront campaign placement (e.g. slide). Maps to marketing_campaigndisplay.
/// </summary>
public class CampaignDisplay
{
    public int Id { get; set; }
    public string DisplayType { get; set; } = "";
    public int SortOrder { get; set; }
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
    public string? Image { get; set; }
    public string LinkUrl { get; set; } = "";
    public string LinkTarget { get; set; } = "_self";
    public string Rules { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CampaignId { get; set; }
    public int? PostId { get; set; }
    public int? WorkspaceId { get; set; }
}
