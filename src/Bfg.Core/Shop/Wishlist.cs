namespace Bfg.Core.Shop;

public class Wishlist
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? CustomerId { get; set; }
    public string SessionKey { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WishlistItem
{
    public int Id { get; set; }
    public int WishlistId { get; set; }
    public int ProductId { get; set; }
    public int? VariantId { get; set; }
    public DateTime CreatedAt { get; set; }
}
