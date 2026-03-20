namespace Bfg.Core.Delivery;

/// <summary>
/// Shipment consignment. Maps to delivery_consignment.
/// </summary>
public class Consignment
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string ConsignmentNumber { get; set; } = "";
    public string TrackingNumber { get; set; } = "";
    public string State { get; set; } = "";
    public DateTime? ShipDate { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? ActualDelivery { get; set; }
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int RecipientAddressId { get; set; }
    public int SenderAddressId { get; set; }
    public int ServiceId { get; set; }
    public int StatusId { get; set; }
    public int? ManifestId { get; set; }
}
