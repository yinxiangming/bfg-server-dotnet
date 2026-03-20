namespace Bfg.Core.Shop;

/// <summary>
/// M2M product ↔ tag. Maps to shop_product_tags (FK column producttag_id).
/// </summary>
public class ProductTagProduct
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ProductTagId { get; set; }
}
