using Bfg.Api.Middleware;
using Bfg.Core;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class ShopEndpoints
{
    public static void MapShopEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Shop").RequireAuthorization();
        group.MapGet("/products", ListProducts);
        group.MapGet("/categories", ListCategories);
        group.MapGet("/stores", ListStores);
        group.MapGet("/carts", ListCarts);
        group.MapGet("/orders", ListOrders);
    }

    private static async Task<IResult> ListProducts(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Products.AsNoTracking().Where(p => p.IsActive);
        if (wid.HasValue) query = query.Where(p => p.WorkspaceId == wid.Value);
        var list = await query.OrderBy(p => p.Name).Select(p => new { id = p.Id, name = p.Name, slug = p.Slug, price = p.Price, sku = p.Sku }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListCategories(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.ProductCategories.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderBy(c => c.SortOrder).Select(c => new { id = c.Id, name = c.Name, slug = c.Slug }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListStores(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Stores.AsNoTracking().Where(s => s.IsActive);
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.Select(s => new { id = s.Id, name = s.Name, code = s.Code }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListCarts(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Carts.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(c => c.CreatedAt).Select(c => new { id = c.Id, status = c.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListOrders(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Orders.AsNoTracking();
        if (wid.HasValue) query = query.Where(o => o.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(o => o.CreatedAt).Select(o => new { id = o.Id, order_number = o.OrderNumber, status = o.Status, total_amount = o.TotalAmount }).ToListAsync(ct);
        return Results.Ok(list);
    }
}
