using System.Text.Json;
using Bfg.Api.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace Bfg.Api.Tests;

public class PaginationTests
{
    [Fact]
    public void FromRequest_uses_defaults_when_query_missing()
    {
        var ctx = new DefaultHttpContext();
        var (page, pageSize) = Pagination.FromRequest(ctx.Request);
        Assert.Equal(1, page);
        Assert.Equal(Pagination.DefaultPageSize, pageSize);
    }

    [Theory]
    [InlineData("2", "10", 2, 10)]
    [InlineData("1", "100", 1, 100)]
    [InlineData("0", "20", 1, 20)] // invalid page falls back to 1
    [InlineData("-1", "5", 1, 5)]
    public void FromRequest_parses_page_and_page_size(string pageQ, string sizeQ, int expectedPage, int expectedSize)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.QueryString = new QueryString($"?page={pageQ}&page_size={sizeQ}");
        var (page, pageSize) = Pagination.FromRequest(ctx.Request);
        Assert.Equal(expectedPage, page);
        Assert.Equal(expectedSize, pageSize);
    }

    [Fact]
    public void FromRequest_caps_page_size_at_max()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.QueryString = new QueryString("?page_size=500");
        var (_, pageSize) = Pagination.FromRequest(ctx.Request);
        Assert.Equal(Pagination.MaxPageSize, pageSize);
    }

    [Fact]
    public void Wrap_sets_count_next_previous()
    {
        var results = new[] { "a", "b" };
        var wrapped = Pagination.Wrap(results, page: 2, pageSize: 2, total: 5);

        var json = JsonSerializer.Serialize(wrapped);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(5, root.GetProperty("count").GetInt32());
        Assert.Equal("?page=3&page_size=2", root.GetProperty("next").GetString());
        Assert.Equal("?page=1&page_size=2", root.GetProperty("previous").GetString());
    }

    [Fact]
    public void Wrap_first_page_has_null_previous()
    {
        var wrapped = Pagination.Wrap(Array.Empty<string>(), page: 1, pageSize: 20, total: 0);
        var json = JsonSerializer.Serialize(wrapped);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("previous").ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public void Wrap_last_page_has_null_next()
    {
        var wrapped = Pagination.Wrap(new[] { "x" }, page: 1, pageSize: 20, total: 1);
        var json = JsonSerializer.Serialize(wrapped);
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("next").ValueKind == JsonValueKind.Null);
    }
}
