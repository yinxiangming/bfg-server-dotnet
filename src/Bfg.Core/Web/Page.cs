namespace Bfg.Core.Web;

public class Page
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Content { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public string Blocks { get; set; } = "[]";
    public int? ParentId { get; set; }
    public string Template { get; set; } = "default";
    public string Status { get; set; } = "draft";
    public string Language { get; set; } = "";
    public DateTime? PublishedAt { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
