namespace Bfg.Core.Delivery;

/// <summary>
/// Shipping carrier. Matches Django delivery.Carrier.
/// </summary>
public class Carrier
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string? CarrierType { get; set; }
    public string Config { get; set; } = "{}";
    public string TestConfig { get; set; } = "{}";
    public bool IsTestMode { get; set; }
    public string TrackingUrlTemplate { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
