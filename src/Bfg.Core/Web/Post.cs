namespace Bfg.Core.Web;

public class Post
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Title { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Content { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public string FeaturedImage { get; set; } = "";
    public int? CategoryId { get; set; }
    public string MetaTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public string Status { get; set; } = "draft";
    public DateTime? PublishedAt { get; set; }
    public int ViewCount { get; set; }
    public bool AllowComments { get; set; } = true;
    public string Language { get; set; } = "en";
    public int? AuthorId { get; set; }
    public int? CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
