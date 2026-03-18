namespace Bfg.Api.Services;

/// <summary>
/// Generates order numbers in format ORD-YYYYMMDD-XXXXX to match Django.
/// </summary>
public static class OrderNumberService
{
    private static readonly Random Rnd = new();

    public static async Task<string> GenerateAsync(Func<string, Task<bool>> existsAsync)
    {
        var dateStr = DateTime.UtcNow.ToString("yyyyMMdd");
        string orderNumber;
        do
        {
            var suffix = new string(Enumerable.Range(0, 5).Select(_ => (char)('0' + Rnd.Next(0, 10))).ToArray());
            orderNumber = $"ORD-{dateStr}-{suffix}";
        } while (await existsAsync(orderNumber).ConfigureAwait(false));

        return orderNumber;
    }
}
