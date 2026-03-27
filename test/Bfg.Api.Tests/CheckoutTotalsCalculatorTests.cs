using Bfg.Api.Services;

namespace Bfg.Api.Tests;

public class CheckoutTotalsCalculatorTests
{
    [Theory]
    [InlineData(100, 5)]
    [InlineData(399.99, 5)]
    [InlineData(0, 5)]
    public void DefaultTaxAmount_below_400_is_flat_5(decimal subtotal, decimal expected)
    {
        Assert.Equal(expected, CheckoutTotalsCalculator.DefaultTaxAmount(subtotal));
    }

    [Theory]
    [InlineData(400, 20)] // 400 * 0.05
    [InlineData(500, 25)]
    [InlineData(1000, 50)]
    public void DefaultTaxAmount_from_400_is_percent_rounded(decimal subtotal, decimal expected)
    {
        Assert.Equal(expected, CheckoutTotalsCalculator.DefaultTaxAmount(subtotal));
    }
}
