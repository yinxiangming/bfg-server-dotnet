namespace Bfg.Core.Common;

/// <summary>
/// Per-workspace settings. Matches Django common.Settings.
/// </summary>
public class Settings
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string SiteName { get; set; } = "";
    public string SiteDescription { get; set; } = "";
    public string Logo { get; set; } = "";
    public string Favicon { get; set; } = "";
    public string DefaultLanguage { get; set; } = "en";
    public string SupportedLanguages { get; set; } = "[]"; // JSON array
    public string DefaultCurrency { get; set; } = "NZD";
    public string DefaultTimezone { get; set; } = "UTC";
    public string ContactEmail { get; set; } = "";
    public string SupportEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public string FacebookUrl { get; set; } = "";
    public string TwitterUrl { get; set; } = "";
    public string InstagramUrl { get; set; } = "";
    public string Features { get; set; } = "{}";
    public string CustomSettings { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
}
