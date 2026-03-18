namespace Bfg.Core.Shop;

/// <summary>
/// M2M join: Product - ProductCategory. Django shop.Product.categories.
/// </summary>
public class ProductCategoryProduct
{
    public int ProductId { get; set; }
    public int ProductCategoryId { get; set; }

    public Product Product { get; set; } = null!;
    public ProductCategory ProductCategory { get; set; } = null!;
}
