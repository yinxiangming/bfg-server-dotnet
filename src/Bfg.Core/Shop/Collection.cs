namespace Bfg.Core.Shop;

public class Collection
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Description { get; set; } = "";
    public string Image { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CollectionProduct
{
    public int CollectionId { get; set; }
    public int ProductId { get; set; }
    public int SortOrder { get; set; }
}
