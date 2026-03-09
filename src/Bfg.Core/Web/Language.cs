namespace Bfg.Core.Web;

public class Language
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
