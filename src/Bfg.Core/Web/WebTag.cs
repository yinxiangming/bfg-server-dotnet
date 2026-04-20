namespace Bfg.Core.Web;

public class WebTag
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Language { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
