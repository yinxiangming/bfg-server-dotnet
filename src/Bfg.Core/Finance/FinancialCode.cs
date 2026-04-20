namespace Bfg.Core.Finance;

public class FinancialCode
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public string Unit { get; set; } = "";
    public string TaxType { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
