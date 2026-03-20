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
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? ShippingAddressId { get; set; }
    public int? BillingAddressId { get; set; }
    /// <summary>Django TextField blank=True but DB column is NOT NULL in some deployments — use empty string.</summary>
    public string CustomerNote { get; set; } = "";
    public string AdminNote { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
