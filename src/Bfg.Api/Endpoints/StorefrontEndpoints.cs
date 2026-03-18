using Bfg.Api.Middleware;
using Bfg.Api.Services;
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
        group.MapGet("/products/{slug}", GetStoreProductBySlug).AllowAnonymous();
        group.MapGet("/categories", ListStoreCategories).AllowAnonymous();
        group.MapGet("/categories/{id:int}", GetStoreCategory).AllowAnonymous();
        group.MapGet("/cart/current/", GetCartCurrent).AllowAnonymous();
        group.MapGet("/cart", GetCartCurrent).AllowAnonymous();
        group.MapPost("/cart/add_item/", StoreCartAddItem).AllowAnonymous();
        group.MapPost("/cart/update_item/", StoreCartUpdateItem).AllowAnonymous();
        group.MapPost("/cart/remove_item/", StoreCartRemoveItem).AllowAnonymous();
        group.MapPost("/cart/clear/", StoreCartClear).AllowAnonymous();
        group.MapPost("/cart/checkout/", StoreCartCheckout);
        group.MapGet("/orders", ListStoreOrders);
        group.MapGet("/orders/{id:int}", GetStoreOrder);
        group.MapPost("/orders/{id:int}/cancel/", CancelStoreOrder);
        group.MapPost("/payments/callback/{gateway}", PaymentCallback).AllowAnonymous();
        group.MapGet("/promo/", Promo).AllowAnonymous();
        group.MapGet("/inventory", StoreInventory).AllowAnonymous();
    }

    private static async Task<IResult> ListStoreProducts(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Products.AsNoTracking().Where(p => p.IsActive && (!wid.HasValue || p.WorkspaceId == wid.Value));
        var list = await query.OrderBy(p => p.Name).Select(p => new { id = p.Id, name = p.Name, slug = p.Slug, price = p.Price.ToString("F2"), description = p.Description, media = new List<object>(), variants = new List<object>() }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetStoreProductBySlug(BfgDbContext db, HttpContext ctx, string slug, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == p.Id).Select(v => new { id = v.Id, sku = v.Sku, name = v.Name, price = v.Price, options = v.Options }).ToListAsync(ct);
        return Results.Ok(new { id = p.Id, name = p.Name, slug = p.Slug, price = p.Price.ToString("F2"), description = p.Description, media = new List<object>(), variants });
    }

    private static async Task<IResult> ListStoreCategories(BfgDbContext db, HttpContext ctx, bool? tree, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.ProductCategories.AsNoTracking().Where(c => c.IsActive && (!wid.HasValue || c.WorkspaceId == wid.Value));
        var list = await query.OrderBy(c => c.SortOrder).Select(c => new { id = c.Id, name = c.Name, slug = c.Slug, image_url = (string?)null, product_count = 0 }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetStoreCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.ProductCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, name = c.Name, slug = c.Slug, image_url = (string?)null, product_count = 0 });
    }

    private static async Task<IResult> GetCartCurrent(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(new { id = 0, workspace = 0, customer = (int?)null, status = "active", items = Array.Empty<object>(), total = "0.00" });
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid.Value && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) return Results.Ok(new { id = 0, workspace = wid.Value, customer = (int?)null, status = "active", items = Array.Empty<object>(), total = "0.00" });
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == cart.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        var total = await db.CartItems.Where(i => i.CartId == cart.Id).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        return Results.Ok(new { id = cart.Id, workspace = cart.WorkspaceId, customer = cart.CustomerId, status = cart.Status, items, total = total.ToString("F2") });
    }

    private static async Task<IResult> StoreCartAddItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<StoreCartItemBody>(ct);
        if (body == null || body.product <= 0) return Results.BadRequest();
        var qty = body.quantity ?? 1;
        if (qty <= 0) return Results.BadRequest();
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid.Value && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) { cart = new Bfg.Core.Shop.Cart { WorkspaceId = wid.Value, Status = "active", SessionKey = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }; db.Carts.Add(cart); await db.SaveChangesAsync(ct); }
        var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == body.product && p.WorkspaceId == wid.Value, ct);
        if (prod == null) return Results.NotFound();
        decimal unitPrice = prod.Price;
        int? variantId = body.variant;
        if (variantId.HasValue) { var v = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == variantId.Value && v.ProductId == body.product, ct); if (v != null) unitPrice = v.Price ?? prod.Price; }
        var existing = await db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == body.product && i.VariantId == variantId, ct);
        if (existing != null) { existing.Quantity += qty; existing.UpdatedAt = DateTime.UtcNow; }
        else db.CartItems.Add(new Bfg.Core.Shop.CartItem { CartId = cart.Id, ProductId = body.product, VariantId = variantId, Quantity = qty, UnitPrice = unitPrice, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == cart.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        var total = await db.CartItems.Where(i => i.CartId == cart.Id).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        return Results.Ok(new { id = cart.Id, workspace = cart.WorkspaceId, customer = cart.CustomerId, status = cart.Status, items, total = total.ToString("F2") });
    }

    private static async Task<IResult> StoreCartUpdateItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<StoreCartUpdateBody>(ct);
        if (body?.item_id <= 0) return Results.BadRequest();
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == body.item_id, ct);
        if (item == null) return Results.NotFound();
        var qty = body.quantity ?? 1;
        if (qty <= 0) { db.CartItems.Remove(item); await db.SaveChangesAsync(ct); }
        else { item.Quantity = qty; item.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.CartId, ct);
        var total = await db.CartItems.Where(i => i.CartId == item.CartId).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == item.CartId).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = item.CartId, workspace = cart?.WorkspaceId ?? 0, customer = cart?.CustomerId, status = cart?.Status ?? "active", items, total = total.ToString("F2") });
    }

    private static async Task<IResult> StoreCartRemoveItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<StoreCartRemoveBody>(ct);
        if (body?.item_id <= 0) return Results.BadRequest();
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == body.item_id, ct);
        if (item == null) return Results.NotFound();
        var cartId = item.CartId;
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cartId, ct);
        var total = await db.CartItems.Where(i => i.CartId == cartId).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == cartId).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = cartId, workspace = cart?.WorkspaceId ?? 0, customer = cart?.CustomerId, status = cart?.Status ?? "active", items, total = total.ToString("F2") });
    }

    private static async Task<IResult> StoreCartClear(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) return Results.Ok(new { id = 0, workspace = wid ?? 0, customer = (int?)null, status = "active", items = Array.Empty<object>(), total = "0.00" });
        var toRemove = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        db.CartItems.RemoveRange(toRemove);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = cart.Id, workspace = cart.WorkspaceId, customer = cart.CustomerId, status = cart.Status, items = Array.Empty<object>(), total = "0.00" });
    }

    private static async Task<IResult> StoreCartCheckout(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<StoreCheckoutBody>(ct);
        if (body == null || body.store <= 0 || body.shipping_address <= 0) return Results.BadRequest(new { detail = "store and shipping_address are required." });
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid.Value && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) return Results.BadRequest(new { detail = "Cart is empty." });
        var cartItems = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        if (!cartItems.Any()) return Results.BadRequest(new { detail = "Cart is empty." });
        var customerId = cart.CustomerId ?? await db.Customers.AsNoTracking().Where(c => c.WorkspaceId == wid.Value).Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });
        var orderNum = await OrderNumberService.GenerateAsync(ord => db.Orders.AnyAsync(o => o.OrderNumber == ord, ct));
        decimal subtotal = cartItems.Sum(i => i.Quantity * i.UnitPrice);
        var order = new Bfg.Core.Shop.Order { WorkspaceId = wid.Value, CustomerId = customerId, StoreId = body.store, OrderNumber = orderNum, Status = "pending", PaymentStatus = "pending", Subtotal = subtotal, ShippingCost = 0, Tax = 0, Discount = 0, TotalAmount = subtotal, ShippingAddressId = body.shipping_address, BillingAddressId = body.billing_address ?? body.shipping_address, CustomerNote = body.customer_note, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        foreach (var it in cartItems)
        {
            var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == it.ProductId, ct);
            var variant = it.VariantId.HasValue ? await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == it.VariantId, ct) : null;
            db.OrderItems.Add(new Bfg.Core.Shop.OrderItem { OrderId = order.Id, ProductId = it.ProductId, VariantId = it.VariantId, ProductName = prod?.Name ?? "", VariantName = variant?.Name ?? "", Sku = variant?.Sku ?? prod?.Sku ?? "", Quantity = it.Quantity, UnitPrice = it.UnitPrice, TotalPrice = it.Quantity * it.UnitPrice, CreatedAt = DateTime.UtcNow });
        }
        db.CartItems.RemoveRange(cartItems);
        await db.SaveChangesAsync(ct);
        var orderItems = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == order.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Created("/api/v1/store/orders/", new { id = order.Id, order_number = order.OrderNumber, workspace = order.WorkspaceId, customer_id = order.CustomerId, store_id = order.StoreId, status = order.Status, payment_status = order.PaymentStatus, subtotal_amount = order.Subtotal.ToString("F2"), shipping_amount = order.ShippingCost.ToString("F2"), discount_amount = order.Discount.ToString("F2"), total_amount = order.TotalAmount.ToString("F2"), shipping_address_id = order.ShippingAddressId, billing_address_id = order.BillingAddressId, items = orderItems, created_at = order.CreatedAt, updated_at = order.UpdatedAt });
    }

    private static async Task<IResult> ListStoreOrders(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Results.Ok(Array.Empty<object>());
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerIds = await db.Customers.AsNoTracking().Where(c => c.UserId == userId && (!wid.HasValue || c.WorkspaceId == wid.Value)).Select(c => c.Id).ToListAsync(ct);
        var list = await db.Orders.AsNoTracking().Where(o => customerIds.Contains(o.CustomerId)).OrderByDescending(o => o.CreatedAt)
            .Select(o => new { id = o.Id, order_number = o.OrderNumber, status = o.Status, total_amount = o.TotalAmount }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetStoreOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Results.NotFound();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerIds = await db.Customers.AsNoTracking().Where(c => c.UserId == userId && (!wid.HasValue || c.WorkspaceId == wid.Value)).Select(c => c.Id).ToListAsync(ct);
        var o = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && customerIds.Contains(x.CustomerId), ct);
        if (o == null) return Results.NotFound();
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, product_name = i.ProductName, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, workspace = o.WorkspaceId, customer_id = o.CustomerId, store_id = o.StoreId, status = o.Status, payment_status = o.PaymentStatus, subtotal_amount = o.Subtotal.ToString("F2"), shipping_amount = o.ShippingCost.ToString("F2"), discount_amount = o.Discount.ToString("F2"), total_amount = o.TotalAmount.ToString("F2"), shipping_address_id = o.ShippingAddressId, billing_address_id = o.BillingAddressId, items, created_at = o.CreatedAt, updated_at = o.UpdatedAt, packages = new List<object>() });
    }

    private static async Task<IResult> CancelStoreOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Results.NotFound();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerIds = await db.Customers.Where(c => c.UserId == userId && (!wid.HasValue || c.WorkspaceId == wid.Value)).Select(c => c.Id).ToListAsync(ct);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && customerIds.Contains(x.CustomerId), ct);
        if (o == null) return Results.NotFound();
        if (o.Status == "delivered" || o.Status == "cancelled" || o.Status == "refunded")
            return Results.BadRequest(new { detail = $"Order in '{o.Status}' status cannot be cancelled." });
        o.Status = "cancelled";
        o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, items });
    }

    private static IResult PaymentCallback(string gateway) => Results.Ok(new { status = "received" });

    private static IResult Promo(HttpRequest req)
    {
        var context = req.Query["context"].ToString() ?? "home";
        return Results.Ok(new { context, available = new { slides = Array.Empty<object>(), flash_sales = Array.Empty<object>(), group_buys = Array.Empty<object>() }, types_present = new[] { "slides", "flash_sales", "group_buys" } });
    }

    private static async Task<IResult> StoreInventory(BfgDbContext db, HttpContext ctx, CancellationToken ct) => Results.Ok(Array.Empty<object>());

    private sealed record StoreCartItemBody(int product, int? variant, int? quantity);
    private sealed record StoreCartUpdateBody(int item_id, int? quantity);
    private sealed record StoreCartRemoveBody(int item_id);
    private sealed record StoreCheckoutBody(int store, int shipping_address, int? billing_address, string? customer_note);
}
