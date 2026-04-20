using System.Security.Claims;
using System.Text.Json;
using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using Bfg.Core.Common;
using Bfg.Core.Finance;
using Bfg.Core.Shop;
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
        group.MapGet("/products/", ListStoreProducts).AllowAnonymous();
        group.MapGet("/products/{slug}/reviews/", ListStoreProductReviews).AllowAnonymous();
        group.MapPost("/products/{slug}/reviews/", CreateStoreProductReview);
        group.MapGet("/products/{slug}/", GetStoreProductBySlug).AllowAnonymous();
        group.MapGet("/categories", ListStoreCategories).AllowAnonymous();
        group.MapGet("/categories/{id:int}", GetStoreCategory).AllowAnonymous();
        group.MapGet("/cart/current/", GetCartCurrent).AllowAnonymous();
        group.MapGet("/cart/preview", StoreCartPreview).AllowAnonymous();
        group.MapGet("/cart/default_store", GetDefaultStore).AllowAnonymous();
        group.MapPost("/cart/add_item/", StoreCartAddItem).AllowAnonymous();
        group.MapPost("/cart/update_item/", StoreCartUpdateItem).AllowAnonymous();
        group.MapPost("/cart/remove_item/", StoreCartRemoveItem).AllowAnonymous();
        group.MapPost("/cart/clear/", StoreCartClear).AllowAnonymous();
        group.MapPost("/cart/checkout/", StoreCartCheckout);
        group.MapPost("/cart/guest_checkout", GuestCartCheckout).AllowAnonymous();
        group.MapGet("/orders/", ListStoreOrders);
        group.MapGet("/orders/{id:int}/", GetStoreOrder);
        group.MapPost("/orders/{id:int}/cancel/", CancelStoreOrder);
        group.MapPost("/payments/intent/", StorePaymentIntent);
        group.MapPost("/payments/{id:long}/process/", StorePaymentProcess);
        group.MapPost("/payments/callback/{gateway}", PaymentCallback).AllowAnonymous();
        group.MapGet("/payments/gateways", ListStorefrontPaymentGateways).AllowAnonymous();
        group.MapGet("/promo/", Promo).AllowAnonymous();
        group.MapGet("/inventory", StoreInventory).AllowAnonymous();
        group.MapGet("/wishlist", GetWishlist);
        group.MapPost("/wishlist/add", AddToWishlist);
        group.MapPost("/wishlist/remove", RemoveFromWishlist);
    }

    private static async Task<IResult> ListStoreProducts(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Array.Empty<object>());
        var q = req.Query;
        var catSlug = q["category"].ToString();
        var search = q["q"].ToString();
        var tagSlug = q["tag"].ToString();
        var minP = decimal.TryParse(q["min_price"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var mn) ? mn : (decimal?)null;
        var maxP = decimal.TryParse(q["max_price"].ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var mx) ? mx : (decimal?)null;
        var featured = string.Equals(q["featured"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
        var isNew = string.Equals(q["is_new"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
        var bestseller = string.Equals(q["bestseller"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
        var limit = int.TryParse(q["limit"].ToString(), out var lim) && lim > 0 && lim <= 500 ? lim : (int?)null;
        var sort = string.IsNullOrEmpty(q["sort"].ToString()) ? "name" : q["sort"].ToString()!;

        var baseQ = db.Products.AsNoTracking().Where(p => p.IsActive && p.WorkspaceId == wid.Value);
        if (featured) baseQ = baseQ.Where(p => p.IsFeatured);
        if (minP.HasValue) baseQ = baseQ.Where(p => p.Price >= minP.Value);
        if (maxP.HasValue) baseQ = baseQ.Where(p => p.Price <= maxP.Value);
        if (isNew)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            baseQ = baseQ.Where(p => p.CreatedAt >= cutoff);
        }
        if (!string.IsNullOrEmpty(catSlug))
        {
            var catId = await db.ProductCategories.AsNoTracking()
                .Where(c => c.WorkspaceId == wid.Value && c.Slug == catSlug)
                .Select(c => c.Id).FirstOrDefaultAsync(ct);
            if (catId == 0) return Results.Ok(Array.Empty<object>());
            var pids = db.ProductCategoryProducts.AsNoTracking().Where(pcp => pcp.ProductCategoryId == catId).Select(pcp => pcp.ProductId);
            baseQ = baseQ.Where(p => pids.Contains(p.Id));
        }
        if (!string.IsNullOrEmpty(tagSlug))
        {
            var tagId = await db.ProductTags.AsNoTracking()
                .Where(t => t.WorkspaceId == wid.Value && t.Slug == tagSlug)
                .Select(t => t.Id).FirstOrDefaultAsync(ct);
            if (tagId == 0) return Results.Ok(Array.Empty<object>());
            var pids = db.ProductTagProducts.AsNoTracking().Where(l => l.ProductTagId == tagId).Select(l => l.ProductId);
            baseQ = baseQ.Where(p => pids.Contains(p.Id));
        }
        if (!string.IsNullOrEmpty(search))
        {
            var term = search.ToLowerInvariant();
            baseQ = baseQ.Where(p => p.Name.ToLower().Contains(term) || p.Slug.ToLower().Contains(term));
        }
        if (bestseller)
        {
            var topIds = await db.OrderItems.AsNoTracking()
                .Join(db.Orders.AsNoTracking(), i => i.OrderId, o => o.Id, (i, o) => new { i.ProductId, o.WorkspaceId })
                .Where(x => x.WorkspaceId == wid.Value)
                .GroupBy(x => x.ProductId)
                .OrderByDescending(g => g.Count())
                .Take(100)
                .Select(g => g.Key)
                .ToListAsync(ct);
            if (topIds.Count == 0) return Results.Ok(Array.Empty<object>());
            baseQ = baseQ.Where(p => topIds.Contains(p.Id));
        }

        baseQ = sort switch
        {
            "price_asc" => baseQ.OrderBy(p => p.Price),
            "price_desc" => baseQ.OrderByDescending(p => p.Price),
            "name" => baseQ.OrderBy(p => p.Name),
            _ => baseQ.OrderBy(p => p.Name)
        };

        if (limit.HasValue) baseQ = baseQ.Take(limit.Value);
        var products = await baseQ.ToListAsync(ct);
        var outList = new List<object>();
        foreach (var p in products)
        {
            var variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == p.Id).OrderBy(v => v.SortOrder)
                .Select(v => new { id = v.Id, sku = v.Sku, name = v.Name, price = v.Price, stock_quantity = v.StockQuantity, is_active = v.IsActive, options = v.Options, compare_price = v.ComparePrice }).ToListAsync(ct);
            var rc = await db.ProductReviews.AsNoTracking().CountAsync(r => r.ProductId == p.Id && r.IsApproved, ct);
            double? ratingAvg = null;
            if (rc > 0)
                ratingAvg = await db.ProductReviews.AsNoTracking().Where(r => r.ProductId == p.Id && r.IsApproved).AverageAsync(r => (double?)r.Rating, ct);
            var ratingOut = ratingAvg ?? 0d;

            var variantObjs = variants.Select(v => (object)new
            {
                v.id,
                v.sku,
                v.name,
                price = v.price.HasValue ? v.price.Value.ToString("F2") : (string?)null,
                stock_quantity = v.stock_quantity,
                v.is_active,
                options = ParseOptionsDict(v.options),
                compare_price = v.compare_price?.ToString("F2")
            }).ToList();

            outList.Add(new
            {
                id = p.Id,
                name = p.Name,
                slug = p.Slug,
                price = p.Price.ToString("F2"),
                description = p.Description,
                media = Array.Empty<object>(),
                variants = variantObjs,
                rating = ratingOut,
                reviews_count = rc,
                primary_image = (string?)null,
                images = Array.Empty<object>(),
                discount_percentage = (decimal?)null,
                is_new = p.CreatedAt >= DateTime.UtcNow.AddDays(-30),
                is_featured = p.IsFeatured
            });
        }
        return Results.Ok(outList);
    }

    private static async Task<IResult> GetStoreProductBySlug(BfgDbContext db, HttpContext ctx, string slug, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == p.Id).OrderBy(v => v.SortOrder)
            .Select(v => new { v.Id, v.Sku, v.Name, v.Price, v.Options, v.ComparePrice, v.StockQuantity, v.IsActive }).ToListAsync(ct);
        var rc = await db.ProductReviews.AsNoTracking().CountAsync(r => r.ProductId == p.Id && r.IsApproved, ct);
        double? ratingAvg = null;
        if (rc > 0)
            ratingAvg = await db.ProductReviews.AsNoTracking().Where(r => r.ProductId == p.Id && r.IsApproved).AverageAsync(r => (double?)r.Rating, ct);
        var ratingOut = ratingAvg ?? 0d;

        var variantObjs = variants.Select(v => (object)new
        {
            id = v.Id,
            sku = v.Sku,
            name = v.Name,
            price = v.Price.HasValue ? v.Price.Value.ToString("F2") : (string?)null,
            stock_quantity = v.StockQuantity,
            is_active = v.IsActive,
            options = ParseOptionsDict(v.Options),
            compare_price = v.ComparePrice?.ToString("F2")
        }).ToList();

        return Results.Ok(new
        {
            id = p.Id,
            name = p.Name,
            slug = p.Slug,
            price = p.Price.ToString("F2"),
            description = p.Description,
            media = Array.Empty<object>(),
            variants = variantObjs,
            rating = ratingOut,
            reviews_count = rc,
            primary_image = (string?)null,
            images = Array.Empty<object>(),
            discount_percentage = (decimal?)null,
            is_new = p.CreatedAt >= DateTime.UtcNow.AddDays(-30),
            is_featured = p.IsFeatured,
            stock_quantity = p.StockQuantity,
            track_inventory = p.TrackInventory,
            in_stock = !p.TrackInventory || p.StockQuantity > 0
        });
    }

    private static object ParseOptionsDict(string json)
    {
        try
        {
            var raw = string.IsNullOrWhiteSpace(json) ? "{}" : json.Trim();
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            if (root.ValueKind == JsonValueKind.String)
            {
                var inner = root.GetString();
                if (!string.IsNullOrEmpty(inner))
                    return ParseOptionsDict(inner);
                return new Dictionary<string, string>();
            }
            if (root.ValueKind != JsonValueKind.Object)
                return new Dictionary<string, string>();
            var flat = new Dictionary<string, string>();
            foreach (var prop in root.EnumerateObject())
            {
                var el = prop.Value;
                flat[prop.Name] = el.ValueKind == JsonValueKind.String ? (el.GetString() ?? "") : el.GetRawText();
            }
            return flat;
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private static async Task<IResult> ListStoreProductReviews(BfgDbContext db, HttpContext ctx, string slug, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var q = req.Query["rating"].ToString();
        int? ratingFilter = int.TryParse(q, out var rf) ? rf : null;
        var ws = p.WorkspaceId;
        var query = db.ProductReviews.AsNoTracking()
            .Where(r => r.ProductId == p.Id && r.WorkspaceId == ws && r.IsApproved);
        if (ratingFilter.HasValue) query = query.Where(r => r.Rating == ratingFilter.Value);
        var rows = await query.OrderByDescending(r => r.CreatedAt)
            .Join(db.Customers.AsNoTracking(), r => r.CustomerId, c => c.Id, (r, c) => new { r, c })
            .Join(db.Users.AsNoTracking(), x => x.c.UserId, u => u.Id, (x, u) => new { x.r, u.Username })
            .Select(x => new { x.r.Id, x.r.Rating, x.r.Title, x.r.Comment, x.r.CreatedAt, x.Username })
            .ToListAsync(ct);
        return Results.Ok(rows.Select(x => (object)new
        {
            id = x.Id,
            rating = x.Rating,
            title = x.Title,
            comment = x.Comment,
            created_at = x.CreatedAt,
            customer_name = x.Username
        }).ToList());
    }

    private static async Task<IResult> CreateStoreProductReview(BfgDbContext db, HttpContext ctx, string slug, CancellationToken ct)
    {
        if (ctx.User?.Identity?.IsAuthenticated != true)
            return Results.Unauthorized();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (uid is null or <= 0) return Results.Unauthorized();
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug && x.WorkspaceId == wid.Value, ct);
        if (p == null) return Results.NotFound();
        var customerId = await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.UserId == uid.Value)
            .Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });
        if (await db.ProductReviews.AnyAsync(r => r.ProductId == p.Id && r.CustomerId == customerId, ct))
            return Results.BadRequest(new { detail = "Duplicate review." });
        var body = await ctx.Request.ReadFromJsonAsync<StoreReviewCreateBody>(ct);
        if (body is not { rating: >= 1 and <= 5 }) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var rev = new ProductReview
        {
            WorkspaceId = wid.Value,
            ProductId = p.Id,
            CustomerId = customerId,
            OrderId = null,
            Rating = body.rating,
            Title = body.title ?? "",
            Comment = body.comment ?? "",
            Images = "[]",
            IsVerifiedPurchase = false,
            IsApproved = true,
            HelpfulCount = 0,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.ProductReviews.Add(rev);
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/store/products/{slug}/reviews/", new { id = rev.Id, rating = rev.Rating, title = rev.Title });
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

    private static async Task<IResult> GetCartCurrent(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue)
        {
            var empty = new CartDetail(0, 0, null, CartService.StatusOpen, Array.Empty<CartLineDto>(), "0.00");
            return Results.Ok(CartJson.StorefrontDetail(empty));
        }

        var sessionKey = StorefrontCartSession.Resolve(ctx);
        var d = await carts.GetCurrentOrEmptyForStorefrontAsync(wid.Value, sessionKey, ct);
        return Results.Ok(CartJson.StorefrontDetail(d));
    }

    /// <summary>GET /store/cart/preview — return cart with computed totals, no auth required.</summary>
    private static async Task<IResult> StoreCartPreview(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue)
        {
            var empty = new CartDetail(0, 0, null, CartService.StatusOpen, Array.Empty<CartLineDto>(), "0.00");
            return Results.Ok(CartJson.StorefrontDetail(empty));
        }

        var sessionKey = StorefrontCartSession.Resolve(ctx);
        var d = await carts.GetCurrentOrEmptyForStorefrontAsync(wid.Value, sessionKey, ct);
        return Results.Ok(CartJson.StorefrontDetail(d));
    }

    /// <summary>GET /store/cart/default_store — return default active store for workspace.</summary>
    private static async Task<IResult> GetDefaultStore(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.NotFound(new { detail = "No workspace." });

        var store = await db.Stores.AsNoTracking()
            .Where(s => s.WorkspaceId == wid.Value && s.IsActive)
            .OrderBy(s => s.Id)
            .Select(s => new { id = s.Id, name = s.Name, workspace = s.WorkspaceId })
            .FirstOrDefaultAsync(ct);

        if (store == null) return Results.NotFound(new { detail = "No active store found." });
        return Results.Ok(store);
    }

    private static async Task<IResult> StoreCartAddItem(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<StoreCartItemBody>(ct);
        if (body is not { Product: > 0 }) return Results.BadRequest();
        var qty = body.Quantity ?? 1;
        if (qty <= 0) return Results.BadRequest(new { detail = "Quantity must be greater than 0." });
        var sessionKey = StorefrontCartSession.Resolve(ctx);
        var r = await carts.AddItemForStorefrontAsync(
            wid.Value, sessionKey, body.Product, body.Variant, qty, new CartAddConstraints(1, null), ct);
        if (!r.Success)
            return r.ErrorCode == "not_found" ? Results.NotFound() : Results.BadRequest(new { detail = r.ErrorMessage });
        return Results.Ok(CartJson.StorefrontDetail(r.Detail!));
    }

    private static async Task<IResult> StoreCartUpdateItem(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<StoreCartUpdateBody>(ct);
        if (body is not { ItemId: > 0 }) return Results.BadRequest();
        var qty = body.Quantity ?? 1;
        var r = await carts.UpdateLineQuantityAsync(body.ItemId, qty, ct);
        if (!r.Success) return Results.NotFound();
        return Results.Ok(CartJson.StorefrontDetail(r.Detail!));
    }

    private static async Task<IResult> StoreCartRemoveItem(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<StoreCartRemoveBody>(ct);
        if (body is not { ItemId: > 0 }) return Results.BadRequest();
        var r = await carts.RemoveLineAsync(body.ItemId, ct);
        if (!r.Success) return Results.NotFound();
        return Results.Ok(CartJson.StorefrontDetail(r.Detail!));
    }

    private static async Task<IResult> StoreCartClear(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue)
            return Results.Ok(new { id = 0, workspace = 0, customer = (int?)null, status = CartService.StatusOpen, items = Array.Empty<object>(), total = "0.00" });
        var sessionKey = StorefrontCartSession.Resolve(ctx);
        var d = await carts.ClearStorefrontCartAsync(wid.Value, sessionKey, ct);
        return Results.Ok(CartJson.ClearedCart(d));
    }

    private static async Task<IResult> StoreCartCheckout(OrderCheckoutService checkout, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        int? cartHeader = null;
        if (ctx.Request.Headers.TryGetValue("X-Cart-Id", out var hv) && int.TryParse(hv.FirstOrDefault(), out var cx) && cx > 0)
            cartHeader = cx;
        var body = await ctx.Request.ReadFromJsonAsync<StoreCheckoutBody>(ct);
        if (body == null || body.Store <= 0 || body.ShippingAddress <= 0)
            return Results.BadRequest(new { detail = "store and shipping_address are required." });
        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        var sessionForCart = cartHeader.HasValue ? null : StorefrontCartSession.Resolve(ctx);
        var r = await checkout.CreateFromCurrentCartAsync(
            wid.Value,
            cartHeader,
            uid,
            body.Store,
            body.ShippingAddress,
            body.BillingAddress,
            body.CustomerNote,
            body.CouponCode,
            body.GiftCardCode,
            validateStoreInWorkspace: true,
            storefrontSessionKey: sessionForCart,
            ct);
        if (!r.Success)
        {
            if (r.ErrorCode == "store_not_found")
                return Results.NotFound(new { detail = r.ErrorMessage });
            return Results.BadRequest(new { detail = r.ErrorMessage });
        }

        return Results.Created("/api/v1/store/orders/", OrderCheckoutJson.CreatedBody(r.Payload!));
    }

    /// <summary>POST /store/cart/guest_checkout — checkout without auth.</summary>
    private static async Task<IResult> GuestCartCheckout(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest(new { detail = "Workspace required." });

        var body = await ctx.Request.ReadFromJsonAsync<GuestCheckoutBody>(ct);
        if (body == null) return Results.BadRequest(new { detail = "Request body required." });
        if (string.IsNullOrWhiteSpace(body.Name))
            return Results.BadRequest(new { detail = "name is required." });
        if (string.IsNullOrWhiteSpace(body.Email))
            return Results.BadRequest(new { detail = "email is required." });
        if (string.IsNullOrWhiteSpace(body.Phone))
            return Results.BadRequest(new { detail = "phone is required." });
        if (body.ShippingAddress == null)
            return Results.BadRequest(new { detail = "shipping_address is required." });

        // Resolve session cart
        var sessionKey = StorefrontCartSession.Resolve(ctx);
        var cart = await db.Carts.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.SessionKey == sessionKey)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(ct);

        List<CartItem> cartItems = new();
        if (cart != null)
            cartItems = await db.CartItems.AsNoTracking().Where(i => i.CartId == cart.Id).ToListAsync(ct);

        if (cartItems.Count == 0)
            return Results.BadRequest(new { detail = "Cart is empty." });

        // Resolve default store
        var store = await db.Stores.AsNoTracking()
            .Where(s => s.WorkspaceId == wid.Value && s.IsActive)
            .OrderBy(s => s.Id)
            .FirstOrDefaultAsync(ct);
        if (store == null)
            return Results.BadRequest(new { detail = "No active store found." });
        var storeId = body.StoreId ?? store.Id;

        // Build order totals
        decimal subtotal = 0m;
        var orderItems = new List<OrderItem>();
        var now = DateTime.UtcNow;

        foreach (var item in cartItems)
        {
            var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == item.ProductId && p.WorkspaceId == wid.Value, ct);
            if (product == null) continue;
            decimal unitPrice = product.Price;
            if (item.VariantId.HasValue)
            {
                var variant = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == item.VariantId.Value, ct);
                if (variant?.Price.HasValue == true) unitPrice = variant.Price.Value;
            }
            var lineTotal = unitPrice * item.Quantity;
            subtotal += lineTotal;
            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                VariantId = item.VariantId,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                TotalPrice = lineTotal,
                CreatedAt = now
            });
        }

        var totalAmount = subtotal; // shipping/tax/discount can be extended later

        // Generate order number
        var orderNumber = "ORD-G-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Encode guest info into CustomerNote as JSON metadata (Order has no dedicated guest fields)
        var guestMeta = JsonSerializer.Serialize(new
        {
            guest = true,
            name = body.Name,
            email = body.Email,
            phone = body.Phone,
            note = body.Note ?? "",
            shipping_address = body.ShippingAddress
        });

        var order = new Order
        {
            WorkspaceId = wid.Value,
            CustomerId = 0, // guest: no customer
            StoreId = storeId,
            OrderNumber = orderNumber,
            Status = "pending",
            PaymentStatus = "unpaid",
            Subtotal = subtotal,
            ShippingCost = 0m,
            Tax = 0m,
            Discount = 0m,
            TotalAmount = totalAmount,
            ShippingAddressId = null,
            BillingAddressId = null,
            CustomerNote = guestMeta,
            AdminNote = "",
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        foreach (var oi in orderItems)
        {
            oi.OrderId = order.Id;
            db.OrderItems.Add(oi);
        }

        // Clear cart items after guest checkout (Cart has no status field — remove items)
        if (cart != null)
        {
            var cartItemsToRemove = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
            db.CartItems.RemoveRange(cartItemsToRemove);
            var trackedCart = await db.Carts.FirstOrDefaultAsync(c => c.Id == cart.Id, ct);
            if (trackedCart != null) trackedCart.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);

        return Results.Created("/api/v1/store/orders/", new
        {
            id = order.Id,
            order_number = order.OrderNumber,
            status = order.Status,
            payment_status = order.PaymentStatus,
            total_amount = order.TotalAmount.ToString("F2"),
            guest_name = body.Name,
            guest_email = body.Email,
            guest_phone = body.Phone,
            items = orderItems.Select(i => new
            {
                product = i.ProductId,
                variant = i.VariantId,
                product_name = i.ProductName,
                quantity = i.Quantity,
                unit_price = i.UnitPrice.ToString("F2"),
                total_price = i.TotalPrice.ToString("F2")
            }).ToList()
        });
    }

    private static async Task<IResult> ListStoreOrders(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Results.Ok(Array.Empty<object>());
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerIds = await db.Customers.AsNoTracking().Where(c => c.UserId == userId && (!wid.HasValue || c.WorkspaceId == wid.Value)).Select(c => c.Id).ToListAsync(ct);
        var statusFilter = req.Query["status"].ToString();
        var query = db.Orders.AsNoTracking().Where(o => customerIds.Contains(o.CustomerId));
        if (!string.IsNullOrEmpty(statusFilter))
            query = query.Where(o => o.Status == statusFilter);
        var list = await query.OrderByDescending(o => o.CreatedAt)
            .Select(o => new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, total_amount = o.TotalAmount }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetStoreOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId)) return Results.NotFound();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerIds = await db.Customers.AsNoTracking().Where(c => c.UserId == userId && (!wid.HasValue || c.WorkspaceId == wid.Value)).Select(c => c.Id).ToListAsync(ct);
        var o = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && customerIds.Contains(x.CustomerId), ct);
        if (o == null) return Results.NotFound();
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, product_name = i.ProductName, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        var packages = await db.DeliveryPackages.AsNoTracking()
            .Where(p => p.OrderId == id)
            .OrderBy(p => p.Id)
            .Select(p => new { p.Id, p.OrderId, p.TemplateId, p.Weight })
            .ToListAsync(ct);
        var pkgObjs = packages.Select(p => (object)new { id = p.Id, order = p.OrderId, order_id = p.OrderId, template = p.TemplateId, template_id = p.TemplateId, weight = p.Weight?.ToString("F2") }).ToList();
        return Results.Ok(new
        {
            id = o.Id,
            order_number = o.OrderNumber,
            workspace = o.WorkspaceId,
            customer_id = o.CustomerId,
            store_id = o.StoreId,
            status = o.Status,
            payment_status = o.PaymentStatus,
            subtotal_amount = o.Subtotal.ToString("F2"),
            shipping_amount = o.ShippingCost.ToString("F2"),
            discount_amount = o.Discount.ToString("F2"),
            total_amount = o.TotalAmount.ToString("F2"),
            amounts = new { subtotal = o.Subtotal.ToString("F2"), shipping_cost = o.ShippingCost.ToString("F2"), tax = o.Tax.ToString("F2"), discount = o.Discount.ToString("F2"), total = o.TotalAmount.ToString("F2") },
            shipping_address_id = o.ShippingAddressId,
            billing_address_id = o.BillingAddressId,
            items,
            created_at = o.CreatedAt,
            updated_at = o.UpdatedAt,
            packages = pkgObjs
        });
    }

    private static async Task<IResult> CancelStoreOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var userIdStr = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
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

    private static async Task<IResult> StorePaymentIntent(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (!wid.HasValue || uid is null or <= 0) return Results.Unauthorized();
        var customerId = await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.UserId == uid.Value)
            .Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });
        var body = await ctx.Request.ReadFromJsonAsync<StorePaymentIntentBody>(ct);
        if (body is not { order_id: > 0, gateway_id: > 0 }) return Results.BadRequest(new { detail = "Invalid body." });
        var order = await db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == body.order_id && o.WorkspaceId == wid.Value, ct);
        if (order == null || order.CustomerId != customerId) return Results.NotFound(new { detail = "Order not found." });
        var gateway = await db.PaymentGateways.AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == body.gateway_id && g.WorkspaceId == wid.Value, ct);
        if (gateway == null) return Results.NotFound(new { detail = "Gateway not found." });
        var currency = await db.Currencies.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Id).FirstOrDefaultAsync(ct);
        if (currency == null) return Results.BadRequest(new { detail = "No active currency." });
        var payNum = "PAY-I-" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var p = new Payment
        {
            WorkspaceId = wid.Value,
            OrderId = order.Id,
            CustomerId = customerId,
            GatewayId = gateway.Id,
            CurrencyId = currency.Id,
            Amount = order.TotalAmount,
            Status = "pending",
            PaymentNumber = payNum,
            GatewayDisplayName = string.IsNullOrEmpty(gateway.Name) ? "Gateway" : gateway.Name,
            GatewayType = string.IsNullOrEmpty(gateway.GatewayType) ? "custom" : gateway.GatewayType,
            GatewayTransactionId = "",
            GatewayResponse = "{}",
            CreatedAt = DateTime.UtcNow
        };
        db.Payments.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/store/payments/intent/", new
        {
            payment_id = p.Id,
            payment_number = p.PaymentNumber,
            amount = p.Amount.ToString("F2"),
            currency = currency.Code,
            gateway_payload = new { gateway_type = gateway.GatewayType },
            status = p.Status
        });
    }

    private static async Task<IResult> StorePaymentProcess(BfgDbContext db, HttpContext ctx, long id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (!wid.HasValue || uid is null or <= 0) return Results.Unauthorized();
        var customerId = await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.UserId == uid.Value)
            .Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.NotFound(new { detail = "Not found." });
        var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid.Value, ct);
        if (p == null) return Results.NotFound(new { detail = "Not found." });
        var order = p.OrderId.HasValue
            ? await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == p.OrderId && o.WorkspaceId == wid.Value, ct)
            : null;
        var owner = p.CustomerId == customerId || (order != null && order.CustomerId == customerId);
        if (!owner) return Results.NotFound(new { detail = "Not found." });
        p.Status = "completed";
        p.CompletedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { status = "completed", detail = "Payment processed." });
    }

    private static IResult PaymentCallback(string gateway)
    {
        if (!string.Equals(gateway, "custom", StringComparison.OrdinalIgnoreCase))
            return Results.NotFound(new { detail = "Not found." });
        return Results.Ok(new { status = "received" });
    }

    /// <summary>GET /store/payments/gateways — list active payment gateways (public info only).</summary>
    private static async Task<IResult> ListStorefrontPaymentGateways(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Array.Empty<object>());

        var gateways = await db.PaymentGateways.AsNoTracking()
            .Where(g => g.WorkspaceId == wid.Value && g.IsActive)
            .OrderBy(g => g.Id)
            .Select(g => (object)new
            {
                id = g.Id,
                name = g.Name,
                gateway_type = g.GatewayType
            })
            .ToListAsync(ct);

        return Results.Ok(gateways);
    }

    private static async Task<IResult> Promo(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var context = req.Query["context"].ToString() ?? "home";
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        object[] slides = Array.Empty<object>();
        object[] groupBuys = Array.Empty<object>();
        if (wid.HasValue)
        {
            slides = await db.CampaignDisplays.AsNoTracking()
                .Where(d => d.WorkspaceId == wid.Value && d.IsActive && d.DisplayType == "slide")
                .OrderBy(d => d.SortOrder)
                .Join(db.Campaigns.AsNoTracking(), d => d.CampaignId, c => c.Id, (d, c) => new { d, c })
                .Where(x => x.c.IsActive)
                .Select(x => (object)new { id = x.d.Id, title = x.d.Title, link_url = x.d.LinkUrl, campaign_id = x.c.Id })
                .ToArrayAsync(ct);
            groupBuys = await db.Campaigns.AsNoTracking()
                .Where(c => c.WorkspaceId == wid.Value && c.IsActive && c.RequiresParticipation)
                .Select(c => (object)new { id = c.Id, name = c.Name, min_participants = c.MinParticipants })
                .ToArrayAsync(ct);
        }
        return Results.Ok(new
        {
            context,
            available = new { slides, flash_sales = Array.Empty<object>(), group_buys = groupBuys },
            types_present = new[] { "slides", "flash_sales", "group_buys" }
        });
    }

    private static async Task<IResult> StoreInventory(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Array.Empty<object>());
        var productIdStr = req.Query["product_id"].ToString();
        var query = db.Products.AsNoTracking().Where(p => p.WorkspaceId == wid.Value && p.IsActive);
        if (!string.IsNullOrEmpty(productIdStr) && int.TryParse(productIdStr, out var pid))
            query = query.Where(p => p.Id == pid);
        var products = await query.Select(p => new { p.Id, p.Name, p.Slug, p.StockQuantity, p.TrackInventory }).ToListAsync(ct);
        var result = new List<object>();
        foreach (var p in products)
        {
            var variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == p.Id && v.IsActive)
                .Select(v => new { id = v.Id, sku = v.Sku, stock_quantity = v.StockQuantity }).ToListAsync(ct);
            result.Add(new
            {
                product_id = p.Id,
                name = p.Name,
                slug = p.Slug,
                stock_quantity = p.StockQuantity,
                track_inventory = p.TrackInventory,
                in_stock = !p.TrackInventory || p.StockQuantity > 0,
                variants
            });
        }
        return Results.Ok(result);
    }

    // ─── Wishlist ────────────────────────────────────────────────────────────────

    /// <summary>GET /store/wishlist — get or create wishlist for current authenticated user.</summary>
    private static async Task<IResult> GetWishlist(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest(new { detail = "Workspace required." });

        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (uid is null or <= 0) return Results.Unauthorized();

        var customerId = await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.UserId == uid.Value)
            .Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });

        var wishlist = await db.Wishlists
            .FirstOrDefaultAsync(w => w.WorkspaceId == wid.Value && w.CustomerId == customerId, ct);

        if (wishlist == null)
        {
            var now = DateTime.UtcNow;
            wishlist = new Wishlist
            {
                WorkspaceId = wid.Value,
                CustomerId = customerId,
                SessionKey = "",
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Wishlists.Add(wishlist);
            await db.SaveChangesAsync(ct);
        }

        var items = await db.WishlistItems.AsNoTracking()
            .Where(i => i.WishlistId == wishlist.Id)
            .Join(db.Products.AsNoTracking(), i => i.ProductId, p => p.Id, (i, p) => new { i, p })
            .Select(x => (object)new
            {
                id = x.i.Id,
                product_id = x.i.ProductId,
                variant_id = x.i.VariantId,
                product_name = x.p.Name,
                product_slug = x.p.Slug,
                price = x.p.Price.ToString("F2"),
                added_at = x.i.CreatedAt
            })
            .ToListAsync(ct);

        return Results.Ok(new
        {
            id = wishlist.Id,
            workspace = wishlist.WorkspaceId,
            customer_id = wishlist.CustomerId,
            items
        });
    }

    /// <summary>POST /store/wishlist/add — add product/variant to wishlist.</summary>
    private static async Task<IResult> AddToWishlist(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest(new { detail = "Workspace required." });

        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (uid is null or <= 0) return Results.Unauthorized();

        var customerId = await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.UserId == uid.Value)
            .Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });

        var body = await ctx.Request.ReadFromJsonAsync<WishlistAddBody>(ct);
        if (body is not { ProductId: > 0 }) return Results.BadRequest(new { detail = "product_id is required." });

        var productExists = await db.Products.AsNoTracking()
            .AnyAsync(p => p.Id == body.ProductId && p.WorkspaceId == wid.Value && p.IsActive, ct);
        if (!productExists) return Results.NotFound(new { detail = "Product not found." });

        var now = DateTime.UtcNow;
        var wishlist = await db.Wishlists
            .FirstOrDefaultAsync(w => w.WorkspaceId == wid.Value && w.CustomerId == customerId, ct);

        if (wishlist == null)
        {
            wishlist = new Wishlist
            {
                WorkspaceId = wid.Value,
                CustomerId = customerId,
                SessionKey = "",
                CreatedAt = now,
                UpdatedAt = now
            };
            db.Wishlists.Add(wishlist);
            await db.SaveChangesAsync(ct);
        }

        // Check if already in wishlist
        var alreadyExists = await db.WishlistItems.AnyAsync(
            i => i.WishlistId == wishlist.Id && i.ProductId == body.ProductId && i.VariantId == body.VariantId, ct);

        if (alreadyExists)
            return Results.Ok(new { detail = "Already in wishlist." });

        var item = new WishlistItem
        {
            WishlistId = wishlist.Id,
            ProductId = body.ProductId,
            VariantId = body.VariantId,
            CreatedAt = now
        };
        db.WishlistItems.Add(item);
        wishlist.UpdatedAt = now;
        await db.SaveChangesAsync(ct);

        return Results.Created("/api/v1/store/wishlist", new
        {
            id = item.Id,
            wishlist_id = wishlist.Id,
            product_id = item.ProductId,
            variant_id = item.VariantId,
            added_at = item.CreatedAt
        });
    }

    /// <summary>POST /store/wishlist/remove — remove item from wishlist by product_id or item_id.</summary>
    private static async Task<IResult> RemoveFromWishlist(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest(new { detail = "Workspace required." });

        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        if (uid is null or <= 0) return Results.Unauthorized();

        var customerId = await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == wid.Value && c.UserId == uid.Value)
            .Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });

        var body = await ctx.Request.ReadFromJsonAsync<WishlistRemoveBody>(ct);
        if (body == null || (!body.ProductId.HasValue && !body.ItemId.HasValue))
            return Results.BadRequest(new { detail = "product_id or item_id is required." });

        var wishlist = await db.Wishlists.AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkspaceId == wid.Value && w.CustomerId == customerId, ct);
        if (wishlist == null) return Results.NotFound(new { detail = "Wishlist not found." });

        WishlistItem? item = null;
        if (body.ItemId.HasValue)
            item = await db.WishlistItems.FirstOrDefaultAsync(i => i.Id == body.ItemId.Value && i.WishlistId == wishlist.Id, ct);
        else if (body.ProductId.HasValue)
            item = await db.WishlistItems.FirstOrDefaultAsync(i => i.ProductId == body.ProductId.Value && i.WishlistId == wishlist.Id, ct);

        if (item == null) return Results.NotFound(new { detail = "Item not found in wishlist." });

        db.WishlistItems.Remove(item);

        var trackedWishlist = await db.Wishlists.FirstOrDefaultAsync(w => w.Id == wishlist.Id, ct);
        if (trackedWishlist != null) trackedWishlist.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Results.Ok(new { detail = "Removed from wishlist." });
    }

    // ─── Request/response records ────────────────────────────────────────────────

    private sealed record StoreCartItemBody(int Product, int? Variant, int? Quantity);
    private sealed record StoreCartUpdateBody(int ItemId, int? Quantity);
    private sealed record StoreCartRemoveBody(int ItemId);
    private sealed record StoreCheckoutBody(int Store, int ShippingAddress, int? BillingAddress, string? CustomerNote, string? CouponCode = null, string? GiftCardCode = null);
    private sealed record StoreReviewCreateBody(int rating, string? title, string? comment);
    private sealed record StorePaymentIntentBody(int order_id, int gateway_id);
    private sealed record GuestCheckoutBody(string? Name, string? Email, string? Phone, GuestShippingAddress? ShippingAddress, int? StoreId = null, string? Note = null);
    private sealed record GuestShippingAddress(string? Line1, string? Line2, string? City, string? State, string? PostalCode, string? Country);
    private sealed record WishlistAddBody(int ProductId, int? VariantId = null);
    private sealed record WishlistRemoveBody(int? ProductId, int? ItemId = null);
}
