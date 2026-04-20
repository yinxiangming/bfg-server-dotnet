namespace Bfg.Core.Web;

public class WebCategory
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public int? ParentId { get; set; }
    public string ContentTypeName { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Color { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string Language { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
