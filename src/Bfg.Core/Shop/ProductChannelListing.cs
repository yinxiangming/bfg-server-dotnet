namespace Bfg.Core.Shop;

/// <summary>
/// Django shop.ProductChannelListing → shop_productchannellisting.
/// </summary>
public class ProductChannelListing
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ChannelId { get; set; }
    public DateTime? AvailableAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
