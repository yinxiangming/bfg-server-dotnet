namespace Bfg.Core.Shop;

/// <summary>
/// Django shop.SalesChannel → shop_saleschannel.
/// </summary>
public class SalesChannel
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string ChannelType { get; set; } = "custom";
    public string Description { get; set; } = "";
    public string Config { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
