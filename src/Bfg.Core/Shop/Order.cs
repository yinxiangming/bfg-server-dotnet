namespace Bfg.Core.Shop;

public class Order
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int CustomerId { get; set; }
    public int StoreId { get; set; }
    public string OrderNumber { get; set; } = "";
    public string Status { get; set; } = "pending";
    public string PaymentStatus { get; set; } = "pending";
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
