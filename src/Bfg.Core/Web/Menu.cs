namespace Bfg.Core.Web;

public class Menu
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Location { get; set; } = "";
    public string Language { get; set; } = "en";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MenuItem
{
    public int Id { get; set; }
    public int MenuId { get; set; }
    public string Title { get; set; } = "";
    public string Label { get; set; } = "";
    public string Url { get; set; } = "";
    public int? PageId { get; set; }
    public int? PostId { get; set; }
    public int? ParentId { get; set; }
    public string Icon { get; set; } = "";
    public string CssClass { get; set; } = "";
    public int SortOrder { get; set; }
    public bool OpenInNewTab { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
