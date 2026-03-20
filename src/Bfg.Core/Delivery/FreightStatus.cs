namespace Bfg.Core.Delivery;

/// <summary>
/// Freight / package workflow status. Maps to delivery_freightstatus.
/// </summary>
public class FreightStatus
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string State { get; set; } = "";
    public string? Description { get; set; }
    public string Color { get; set; } = "#000000";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true;
    public bool SendMessage { get; set; }
    public int? MappedConsignmentStatusId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
