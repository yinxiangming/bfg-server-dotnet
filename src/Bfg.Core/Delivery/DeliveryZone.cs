namespace Bfg.Core.Delivery;

/// <summary>
/// Delivery zone for freight. Matches Django delivery.DeliveryZone.
/// </summary>
public class DeliveryZone
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
