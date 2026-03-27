using System.Text.RegularExpressions;
using Bfg.Api.Services;

namespace Bfg.Api.Tests;

public class OrderNumberServiceTests
{
    [Fact]
    public async Task GenerateAsync_matches_ord_prefix_and_date_and_suffix()
    {
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
        var pattern = new Regex(@"^ORD-" + dateStr + @"-\d{5}$");
        var n = await OrderNumberService.GenerateAsync(_ => Task.FromResult(false));
        Assert.Matches(pattern, n);
    }

    [Fact]
    public async Task GenerateAsync_retries_until_unique()
    {
        var calls = 0;
        var n = await OrderNumberService.GenerateAsync(_ =>
        {
            calls++;
            return Task.FromResult(calls < 3);
        });
        Assert.Equal(3, calls);
        Assert.StartsWith("ORD-", n);
    }
}
