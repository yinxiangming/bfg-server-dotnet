namespace Bfg.Core.Shop;

public class Product
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Sku { get; set; } = "";
    public string ProductType { get; set; } = "physical";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
