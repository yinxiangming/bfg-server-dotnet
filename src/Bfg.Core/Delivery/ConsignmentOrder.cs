namespace Bfg.Core.Delivery;

/// <summary>
/// M2M consignment ↔ order. Maps to delivery_consignment_orders.
/// </summary>
public class ConsignmentOrder
{
    public int Id { get; set; }
    public int ConsignmentId { get; set; }
    public int OrderId { get; set; }
}
