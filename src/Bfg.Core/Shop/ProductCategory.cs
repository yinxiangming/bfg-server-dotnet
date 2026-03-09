namespace Bfg.Core.Shop;

public class ProductCategory
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int? ParentId { get; set; }
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
