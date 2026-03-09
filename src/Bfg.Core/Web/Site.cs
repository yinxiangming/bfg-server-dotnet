namespace Bfg.Core.Web;

public class Site
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Domain { get; set; } = "";
    public int? ThemeId { get; set; }
    public string DefaultLanguage { get; set; } = "en";
    public string Languages { get; set; } = "[]";
    public string SiteTitle { get; set; } = "";
    public string SiteDescription { get; set; } = "";
    public string NotificationConfig { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
