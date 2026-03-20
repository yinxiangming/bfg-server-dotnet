namespace Bfg.Core.Delivery;

/// <summary>
/// Physical package linked to an order. Maps to delivery_package.
/// </summary>
public class DeliveryPackage
{
    public int Id { get; set; }
    public string PackageNumber { get; set; } = "";
    public decimal? Weight { get; set; }
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public int? Pieces { get; set; }
    public string State { get; set; } = "";
    public string Description { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int? ConsignmentId { get; set; }
    public int? OrderId { get; set; }
    public int StatusId { get; set; }
    public int? TemplateId { get; set; }
    public int? StorageLocationId { get; set; }
}
