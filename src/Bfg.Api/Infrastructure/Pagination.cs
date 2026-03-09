namespace Bfg.Api.Infrastructure;

/// <summary>
/// DRF-style pagination: ?page=1&page_size=20. Use with list endpoints for consistent behaviour.
/// </summary>
public static class Pagination
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) FromRequest(HttpRequest request)
    {
        var page = 1;
        var pageSize = DefaultPageSize;
        if (int.TryParse(request.Query["page"], out var p) && p > 0) page = p;
        if (int.TryParse(request.Query["page_size"], out var ps) && ps > 0) pageSize = Math.Min(ps, MaxPageSize);
        return (page, pageSize);
    }

    public static object Wrap<T>(IReadOnlyList<T> results, int page, int pageSize, int total)
    {
        return new
        {
            count = total,
            next = page * pageSize < total ? (int?)page + 1 : null,
            previous = page > 1 ? (int?)page - 1 : null,
            results
        };
    }
}
