namespace Bfg.Core.Finance;

/// <summary>
/// Payment gateway. Matches Django finance.PaymentGateway.
/// </summary>
public class PaymentGateway
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string GatewayType { get; set; } = "custom";
    public string Config { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public bool IsTestMode { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
