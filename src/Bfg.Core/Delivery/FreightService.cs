namespace Bfg.Core.Delivery;

/// <summary>
/// Shipping service/method. Matches Django delivery.FreightService (no created_at/updated_at in DB).
/// </summary>
public class FreightService
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int CarrierId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal BasePrice { get; set; }
    public decimal PricePerKg { get; set; }
    public int EstimatedDaysMin { get; set; } = 1;
    public int EstimatedDaysMax { get; set; } = 7;
    public decimal MinWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public string Config { get; set; } = "{}";
    public string TransportType { get; set; } = "";
    public bool IsActive { get; set; } = true;
    /// <summary>Django column name: order</summary>
    public int SortOrder { get; set; } = 100;

    public Carrier Carrier { get; set; } = null!;
}
