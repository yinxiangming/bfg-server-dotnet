namespace Bfg.Core.Shop;

/// <summary>
/// Product tag for filtering. Maps to shop_producttag.
/// </summary>
public class ProductTag
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Language { get; set; } = "en";
}
