namespace Bfg.Core.Common;

/// <summary>
/// Customer profile (User per Workspace). Matches Django common.Customer.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int UserId { get; set; }
    public string CustomerNumber { get; set; } = "";
    public string CompanyName { get; set; } = "";
    public string TaxNumber { get; set; } = "";
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string Notes { get; set; } = "";
    public string GatewayMetadata { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
}
