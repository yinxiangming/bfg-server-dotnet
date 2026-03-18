namespace Bfg.Core.Delivery;

/// <summary>
/// Shipment for an order. Matches Django delivery.Shipment/Consignment concept.
/// </summary>
public class Shipment
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int OrderId { get; set; }
    public int? CarrierId { get; set; }
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
