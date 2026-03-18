namespace Bfg.Core.Finance;

/// <summary>
/// Currency. Matches Django finance.Currency.
/// </summary>
public class Currency
{
    public int Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Symbol { get; set; } = "";
    public int DecimalPlaces { get; set; } = 2;
    public bool IsActive { get; set; } = true;
}
