using Bfg.Api.Middleware;
using Bfg.Core;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

/// <summary>
/// Storefront API under /api/v1/store/ (customer-facing). Some routes allow anonymous.
/// </summary>
public static class StorefrontEndpoints
{
    public static void MapStorefrontEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/store").WithTags("Storefront");
        group.MapGet("/products", ListStoreProducts).AllowAnonymous();
        group.MapGet("/categories", ListStoreCategories).AllowAnonymous();
        group.MapGet("/cart", GetCart).AllowAnonymous();
        group.MapGet("/orders", ListStoreOrders).RequireAuthorization();
        group.MapPost("/payments/callback/{gateway}", PaymentCallback).AllowAnonymous();
        group.MapGet("/promo/", Promo).AllowAnonymous();
    }

    private static async Task<IResult> ListStoreProducts(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Products.AsNoTracking().Where(p => p.IsActive);
        if (wid.HasValue) query = query.Where(p => p.WorkspaceId == wid.Value);
        var list = await query.OrderBy(p => p.Name).Select(p => new { id = p.Id, name = p.Name, slug = p.Slug, price = p.Price }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListStoreCategories(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.ProductCategories.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderBy(c => c.SortOrder).Select(c => new { id = c.Id, name = c.Name, slug = c.Slug }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static IResult GetCart() => Results.Ok(new { items = Array.Empty<object>() });

    private static async Task<IResult> ListStoreOrders(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Results.Ok(Array.Empty<object>());
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerIds = await db.Customers.AsNoTracking().Where(c => c.UserId == userId && (!wid.HasValue || c.WorkspaceId == wid.Value)).Select(c => c.Id).ToListAsync(ct);
        var list = await db.Orders.AsNoTracking().Where(o => customerIds.Contains(o.CustomerId)).OrderByDescending(o => o.CreatedAt)
            .Select(o => new { id = o.Id, order_number = o.OrderNumber, status = o.Status, total_amount = o.TotalAmount }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static IResult PaymentCallback(string gateway) => Results.Ok(new { received = true, gateway });

    private static IResult Promo() => Results.Ok(new { campaigns = Array.Empty<object>() });
}
