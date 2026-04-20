namespace Bfg.Core.Finance;

public class Brand
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Logo { get; set; } = "";
    public int? AddressId { get; set; }
    public bool IsDefault { get; set; }
    public string TaxId { get; set; } = "";
    public string RegistrationNumber { get; set; } = "";
    public string InvoiceNote { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
