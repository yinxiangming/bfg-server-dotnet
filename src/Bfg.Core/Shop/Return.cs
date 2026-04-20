namespace Bfg.Core.Shop;

public class Return
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public string ReturnNumber { get; set; } = "";
    public string Status { get; set; } = "pending";
    public string ReasonCategory { get; set; } = "";
    public string CustomerNote { get; set; } = "";
    public string AdminNote { get; set; } = "";
    public int? RefundId { get; set; }
    public int? CreatedById { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ReturnItem
{
    public int Id { get; set; }
    public int ReturnRequestId { get; set; }
    public int OrderItemId { get; set; }
    public int Quantity { get; set; } = 1;
    public string Reason { get; set; } = "";
    public string RestockAction { get; set; } = "";
}
