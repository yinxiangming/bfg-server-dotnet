namespace Bfg.Core.Shop;

public class Product
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Sku { get; set; } = "";
    public string Barcode { get; set; } = "";
    public string ProductType { get; set; } = "physical";
    public string Description { get; set; } = "";
    public string ShortDescription { get; set; } = "";
    public decimal Price { get; set; }
    public decimal? ComparePrice { get; set; }
    public decimal? Cost { get; set; }
    public bool IsSubscription { get; set; }
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public bool RequiresShipping { get; set; } = true;
    public decimal? Weight { get; set; }
    public string MetaTitle { get; set; } = "";
    public string MetaDescription { get; set; } = "";
    public string Condition { get; set; } = "new";
    public int? FinanceCodeId { get; set; }
    public int? SubscriptionPlanId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public string Language { get; set; } = "en";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
