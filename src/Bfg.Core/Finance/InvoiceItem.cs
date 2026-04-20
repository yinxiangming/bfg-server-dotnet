namespace Bfg.Core.Finance;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Description { get; set; } = "";
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; } = 1;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public string TaxType { get; set; } = "";
    public int? FinancialCodeId { get; set; }
    public int? ProductId { get; set; }
}
