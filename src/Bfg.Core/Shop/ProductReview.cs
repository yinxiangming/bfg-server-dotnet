namespace Bfg.Core.Shop;

/// <summary>
/// Customer product review. Maps to shop_productreview.
/// </summary>
public class ProductReview
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int ProductId { get; set; }
    public int CustomerId { get; set; }
    public int? OrderId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = "";
    public string Comment { get; set; } = "";
    public string Images { get; set; } = "[]";
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; } = true;
    public int HelpfulCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
