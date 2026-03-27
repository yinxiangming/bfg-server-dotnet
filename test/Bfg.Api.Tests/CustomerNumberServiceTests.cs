using Bfg.Api.Services;

namespace Bfg.Api.Tests;

public class CustomerNumberServiceTests
{
    [Theory]
    [InlineData(0, "CUST-00001")]
    [InlineData(42, "CUST-00043")]
    [InlineData(99998, "CUST-99999")]
    public async Task GetNextForWorkspaceAsync_formats_counter(int maxSeq, string expected)
    {
        var result = await CustomerNumberService.GetNextForWorkspaceAsync(1, _ => Task.FromResult(maxSeq));
        Assert.Equal(expected, result);
    }
}
