namespace Bfg.Core.Shop;

public class Cart
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? CustomerId { get; set; }
    public int? StoreId { get; set; }
    public string SessionKey { get; set; } = "";
    public string Status { get; set; } = "open";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
