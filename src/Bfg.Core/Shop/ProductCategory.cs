namespace Bfg.Core.Shop;

public class ProductCategory
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public int? ParentId { get; set; }
    public string Language { get; set; } = "en";
    public string Description { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Image { get; set; } = "";
    /// <summary>Django column name is <c>order</c> (mapped in BfgDbContext).</summary>
    public int SortOrder { get; set; }
    public string Rules { get; set; } = "[]";
    public string RuleMatchType { get; set; } = "all";
    public bool IsActive { get; set; } = true;
}
