namespace Bfg.Core.Finance;

/// <summary>
/// Customer saved payment method. Matches Django finance.PaymentMethod.
/// </summary>
public class PaymentMethod
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public int CustomerId { get; set; }
    public int GatewayId { get; set; }
    public string MethodType { get; set; } = "card";
    public string? GatewayToken { get; set; }
    public string CardholderName { get; set; } = "";
    public string? CardBrand { get; set; }
    public string? CardLast4 { get; set; }
    public int? CardExpMonth { get; set; }
    public int? CardExpYear { get; set; }
    public string? DisplayInfo { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
