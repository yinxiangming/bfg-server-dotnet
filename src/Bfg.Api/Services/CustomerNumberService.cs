namespace Bfg.Api.Services;

/// <summary>
/// Generates customer_number in format CUST-00001 (zero-padded counter per workspace) to match Django.
/// </summary>
public static class CustomerNumberService
{
    public static async Task<string> GetNextForWorkspaceAsync(int workspaceId, Func<int, Task<int>> getMaxSequenceAsync)
    {
        var maxSeq = await getMaxSequenceAsync(workspaceId).ConfigureAwait(false);
        var next = maxSeq + 1;
        return $"CUST-{next:D5}";
    }
}
