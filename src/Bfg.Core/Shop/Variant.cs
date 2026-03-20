namespace Bfg.Core.Shop;

/// <summary>
/// Product variant (size, color, etc.). Matches Django shop.ProductVariant.
/// </summary>
public class Variant
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public string Options { get; set; } = "{}";
    public decimal? Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public int StockQuantity { get; set; }
    public decimal? Weight { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>Django column name: order</summary>
    public int SortOrder { get; set; } = 100;

    public Product Product { get; set; } = null!;
}
