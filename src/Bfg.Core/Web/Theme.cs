namespace Bfg.Core.Web;

public class Theme
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string TemplatePath { get; set; } = "";
    public string Logo { get; set; } = "";
    public string Favicon { get; set; } = "";
    public string PrimaryColor { get; set; } = "#007bff";
    public string SecondaryColor { get; set; } = "#6c757d";
    public string Config { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
