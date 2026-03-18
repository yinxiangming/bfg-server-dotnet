namespace Bfg.Core.Delivery;

/// <summary>
/// Shipping service/method. Matches Django delivery.FreightService.
/// </summary>
public class FreightService
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int CarrierId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public decimal BasePrice { get; set; }
    public decimal PricePerKg { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 100;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Carrier Carrier { get; set; } = null!;
}
