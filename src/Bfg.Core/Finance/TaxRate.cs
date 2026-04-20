namespace Bfg.Core.Finance;

public class TaxRate
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public decimal Rate { get; set; }
    public string Country { get; set; } = "";
    public string State { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
