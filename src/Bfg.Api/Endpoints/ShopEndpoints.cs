using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using Bfg.Core.Shop;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class ShopEndpoints
{
    public static void MapShopEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/shop").WithTags("Shop").RequireAuthorization();

        group.MapGet("/categories", ListCategories);
        group.MapPost("/categories/", CreateCategory);
        group.MapGet("/categories/{id:int}", GetCategory);
        group.MapPatch("/categories/{id:int}", PatchCategory);
        group.MapDelete("/categories/{id:int}", DeleteCategory);

        group.MapGet("/products", ListProducts);
        group.MapPost("/products/", CreateProduct);
        group.MapGet("/products/{id:int}", GetProduct);
        group.MapPatch("/products/{id:int}", PatchProduct);
        group.MapDelete("/products/{id:int}", DeleteProduct);

        group.MapGet("/variants", ListVariants);
        group.MapPost("/variants/", CreateVariant);
        group.MapGet("/variants/{id:int}", GetVariant);
        group.MapPatch("/variants/{id:int}", PatchVariant);

        group.MapGet("/stores", ListStores);
        group.MapPost("/stores/", CreateStore);
        group.MapGet("/stores/{id:int}", GetStore);
        group.MapPatch("/stores/{id:int}", PatchStore);
        group.MapPost("/stores/{id:int}/warehouses/", SetStoreWarehouses);

        group.MapGet("/carts", ListCarts);
        group.MapGet("/carts/{id:int}", GetCart);
        group.MapPost("/carts/", CreateCart);
        group.MapPost("/carts/add_item/", CartAddItem);
        group.MapPost("/carts/remove_item/", CartRemoveItem);
        group.MapPost("/carts/clear/", CartClear);
        group.MapPost("/carts/checkout/", CartCheckout);

        group.MapGet("/orders", ListOrders);
        group.MapPost("/orders/", CreateOrder);
        group.MapGet("/orders/{id:int}", GetOrder);
        group.MapPatch("/orders/{id:int}", PatchOrder);
        group.MapPost("/orders/{id:int}/update_status/", OrderUpdateStatus);
        group.MapPost("/orders/{id:int}/cancel/", OrderCancel);
        group.MapPost("/orders/{id:int}/process/", OrderProcess);
        group.MapPost("/orders/{id:int}/ship/", OrderShip);
        group.MapPost("/orders/{id:int}/complete/", OrderComplete);
        group.MapPost("/orders/{id:int}/update_items/", OrderUpdateItems);
        group.MapGet("/order-items", ListOrderItems);
        group.MapPost("/order-items/", CreateOrderItem);
        group.MapGet("/order-items/{id:int}", GetOrderItem);
    }

    private static async Task<IResult> ListCategories(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.ProductCategories.AsNoTracking().Where(c => !wid.HasValue || c.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var (page, pageSize) = Pagination.FromRequest(req);
        var list = await query.OrderBy(c => c.SortOrder).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new { id = c.Id, name = c.Name, slug = c.Slug, language = c.Language, parent = c.ParentId, is_active = c.IsActive }).ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateCategory(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CategoryCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var c = new ProductCategory { WorkspaceId = wid.Value, Name = body.name ?? "", Slug = body.slug ?? "", Language = body.language ?? "en", ParentId = body.parent, IsActive = true, SortOrder = 0, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.ProductCategories.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/shop/categories/", new { id = c.Id, name = c.Name, slug = c.Slug, language = c.Language });
    }

    private static async Task<IResult> GetCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.ProductCategories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, name = c.Name, slug = c.Slug });
    }

    private static async Task<IResult> PatchCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.ProductCategories.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CategoryPatchBody>(ct);
        if (body?.name != null) c.Name = body.name;
        c.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = c.Id, name = c.Name, slug = c.Slug });
    }

    private static async Task<IResult> DeleteCategory(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.ProductCategories.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        db.ProductCategories.Remove(c);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListProducts(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Products.AsNoTracking().Where(p => (!wid.HasValue || p.WorkspaceId == wid.Value));
        var isActive = req.Query["is_active"].FirstOrDefault();
        if (string.IsNullOrEmpty(isActive) || isActive == "true") query = query.Where(p => p.IsActive);
        else if (isActive == "false") query = query.Where(p => !p.IsActive);
        var categoryId = req.Query["category"].FirstOrDefault();
        if (!string.IsNullOrEmpty(categoryId) && int.TryParse(categoryId, out var cid))
            query = query.Where(p => db.ProductCategoryProducts.Any(pcp => pcp.ProductId == p.Id && pcp.ProductCategoryId == cid));
        var lang = req.Query["lang"].FirstOrDefault();
        if (!string.IsNullOrEmpty(lang)) query = query.Where(p => p.Language == lang);
        var search = req.Query["search"].FirstOrDefault();
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p => (p.Name != null && p.Name.Contains(search)) || (p.Sku != null && p.Sku.Contains(search)) || db.Variants.Any(v => v.ProductId == p.Id && v.Sku != null && v.Sku.Contains(search)));
        var total = await query.CountAsync(ct);
        var (page, pageSize) = Pagination.FromRequest(req);
        var list = await query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new { id = p.Id, workspace = p.WorkspaceId, name = p.Name, slug = p.Slug, sku = p.Sku, description = p.Description, short_description = p.ShortDescription, price = p.Price.ToString("F2"), compare_at_price = p.ComparePrice.HasValue ? p.ComparePrice.Value.ToString("F2") : (string?)null, is_active = p.IsActive, language = p.Language, created_at = p.CreatedAt }).ToListAsync(ct);
        var result = new List<object>();
        foreach (var p in list)
        {
            var categoryIds = await db.ProductCategoryProducts.Where(pcp => pcp.ProductId == p.id).Select(pcp => pcp.ProductCategoryId).ToListAsync(ct);
            result.Add(new { p.id, p.workspace, p.name, p.slug, p.sku, p.description, p.short_description, p.price, p.compare_at_price, category_ids = categoryIds, p.is_active, p.language, p.created_at });
        }
        return Results.Ok(Pagination.Wrap(result, page, pageSize, total));
    }

    private static string Slugify(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "product";
        var slug = new string(name.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray()).Trim();
        slug = string.Join("-", slug.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries));
        return slug.Length > 0 ? slug : "product";
    }

    private static async Task<IResult> CreateProduct(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest(new { detail = "No workspace. Send X-Workspace-Id header." });
        var body = await ctx.Request.ReadFromJsonAsync<ProductCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var price = decimal.TryParse(body.price, out var pv) ? pv : 0;
        if (price < 0) return Results.BadRequest(new { price = new[] { "Negative price not allowed." } });
        if (price > 999999.99m) return Results.BadRequest(new { price = new[] { "Price cannot exceed 999999.99." } });
        var language = body.language ?? "en";
        var baseSlug = !string.IsNullOrWhiteSpace(body.slug) ? Slugify(body.slug) : Slugify(body.name ?? "");
        var slug = baseSlug;
        var counter = 1;
        while (await db.Products.AnyAsync(p => p.WorkspaceId == wid.Value && p.Slug == slug && p.Language == language, ct))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }
        var prod = new Product
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Slug = slug,
            Sku = body.sku ?? "",
            Description = body.description ?? "",
            ShortDescription = body.short_description ?? "",
            Price = price,
            ComparePrice = decimal.TryParse(body.compare_at_price, out var cp) ? cp : (decimal?)null,
            Language = language,
            TrackInventory = body.track_inventory ?? true,
            StockQuantity = body.stock_quantity ?? 0,
            IsActive = body.is_active ?? true,
            IsFeatured = body.is_featured ?? false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Products.Add(prod);
        await db.SaveChangesAsync(ct);
        if (body.category_ids != null && body.category_ids.Count > 0)
        {
            foreach (var cid in body.category_ids)
                db.ProductCategoryProducts.Add(new ProductCategoryProduct { ProductId = prod.Id, ProductCategoryId = cid });
            await db.SaveChangesAsync(ct);
        }
        return Results.Created("/api/v1/shop/products/", new { id = prod.Id, workspace = prod.WorkspaceId, name = prod.Name, slug = prod.Slug, sku = prod.Sku, description = prod.Description, short_description = prod.ShortDescription, price = prod.Price.ToString("F2"), compare_at_price = prod.ComparePrice?.ToString("F2"), category_ids = body.category_ids ?? new List<int>(), is_active = prod.IsActive, language = prod.Language, created_at = prod.CreatedAt });
    }

    private static async Task<IResult> GetProduct(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var categoryIds = await db.ProductCategoryProducts.Where(pcp => pcp.ProductId == id).Select(pcp => pcp.ProductCategoryId).ToListAsync(ct);
        var variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == id).OrderBy(v => v.SortOrder).Select(v => new { id = v.Id, sku = v.Sku, name = v.Name, price = v.Price.HasValue ? v.Price.Value.ToString("F2") : (string?)null, stock_quantity = v.StockQuantity, is_active = v.IsActive }).ToListAsync(ct);
        return Results.Ok(new { id = p.Id, workspace = p.WorkspaceId, name = p.Name, slug = p.Slug, sku = p.Sku, description = p.Description, short_description = p.ShortDescription, price = p.Price.ToString("F2"), compare_at_price = p.ComparePrice?.ToString("F2"), category_ids = categoryIds, is_active = p.IsActive, language = p.Language, created_at = p.CreatedAt, variants });
    }

    private static async Task<IResult> PatchProduct(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<ProductPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) p.Name = body.name;
            if (body.slug != null) p.Slug = body.slug;
            if (body.sku != null) p.Sku = body.sku;
            if (body.description != null) p.Description = body.description;
            if (body.short_description != null) p.ShortDescription = body.short_description;
            if (body.price != null && decimal.TryParse(body.price, out var pr)) p.Price = pr;
            if (body.compare_at_price != null) p.ComparePrice = decimal.TryParse(body.compare_at_price, out var cp) ? cp : (decimal?)null;
            if (body.is_active.HasValue) p.IsActive = body.is_active.Value;
            if (body.language != null) p.Language = body.language;
            if (body.category_ids != null) { var existing = await db.ProductCategoryProducts.Where(pcp => pcp.ProductId == id).ToListAsync(ct); db.ProductCategoryProducts.RemoveRange(existing); foreach (var cid in body.category_ids) db.ProductCategoryProducts.Add(new ProductCategoryProduct { ProductId = id, ProductCategoryId = cid }); }
            p.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        var categoryIds = await db.ProductCategoryProducts.Where(pcp => pcp.ProductId == id).Select(pcp => pcp.ProductCategoryId).ToListAsync(ct);
        return Results.Ok(new { id = p.Id, workspace = p.WorkspaceId, name = p.Name, slug = p.Slug, sku = p.Sku, description = p.Description, short_description = p.ShortDescription, price = p.Price.ToString("F2"), compare_at_price = p.ComparePrice?.ToString("F2"), category_ids = categoryIds, is_active = p.IsActive, language = p.Language, created_at = p.CreatedAt });
    }

    private static async Task<IResult> DeleteProduct(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        db.Products.Remove(p);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListVariants(BfgDbContext db, HttpContext ctx, int? product, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var productIds = wid.HasValue ? await db.Products.Where(p => p.WorkspaceId == wid.Value).Select(p => p.Id).ToListAsync(ct) : null;
        var query = db.Variants.AsNoTracking().Where(v => (productIds == null || productIds.Contains(v.ProductId)) && (!product.HasValue || v.ProductId == product.Value));
        var list = await query.OrderBy(v => v.SortOrder).Select(v => new { id = v.Id, product = v.ProductId, sku = v.Sku, name = v.Name, price = v.Price, options = v.Options, stock_quantity = v.StockQuantity, is_active = v.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateVariant(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<VariantCreateBody>(ct);
        if (body == null || body.product <= 0) return Results.BadRequest();
        var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == body.product && p.WorkspaceId == wid.Value, ct);
        if (prod == null) return Results.NotFound();
        var price = body.price != null && decimal.TryParse(body.price, out var pv) ? pv : (decimal?)null;
        var v = new Variant
        {
            ProductId = body.product,
            Sku = body.sku ?? "",
            Name = body.name ?? "",
            Options = body.options != null ? System.Text.Json.JsonSerializer.Serialize(body.options) : "{}",
            Price = price ?? prod.Price,
            StockQuantity = body.stock_quantity ?? 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Variants.Add(v);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/shop/variants/", new { id = v.Id, product = v.ProductId, sku = v.Sku, name = v.Name, price = v.Price?.ToString("F2") });
    }

    private static async Task<IResult> GetVariant(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var v = await db.Variants.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (v == null) return Results.NotFound();
        return Results.Ok(new { id = v.Id, product = v.ProductId, sku = v.Sku, name = v.Name, price = v.Price });
    }

    private static async Task<IResult> PatchVariant(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var v = await db.Variants.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (v == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<VariantPatchBody>(ct);
        if (body != null) { v.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
        return Results.Ok(new { id = v.Id });
    }

    private static async Task<IResult> ListStores(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Stores.AsNoTracking().Where(s => (!wid.HasValue || s.WorkspaceId == wid.Value));
        var list = await query.OrderBy(s => s.Name).Select(s => new { id = s.Id, workspace = s.WorkspaceId, name = s.Name, code = s.Code, description = s.Description ?? "", is_active = s.IsActive }).ToListAsync(ct);
        var result = new List<object>();
        foreach (var s in list)
        {
            var whIds = await db.StoreWarehouses.Where(sw => sw.StoreId == s.id).Select(sw => sw.WarehouseId).ToListAsync(ct);
            var warehouses = await db.Warehouses.AsNoTracking().Where(w => whIds.Contains(w.Id)).Select(w => new { id = w.Id, name = w.Name, code = w.Code }).ToListAsync(ct);
            result.Add(new { id = s.id, workspace = s.workspace, name = s.name, code = s.code, description = s.description, is_active = s.is_active, warehouses });
        }
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateStore(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<StoreCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var s = new Store { WorkspaceId = wid.Value, Name = body.name ?? "", Code = body.code ?? "", Description = body.description ?? "", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Stores.Add(s);
        await db.SaveChangesAsync(ct);
        if (body.warehouse_ids != null)
            foreach (var whId in body.warehouse_ids)
                db.StoreWarehouses.Add(new StoreWarehouse { StoreId = s.Id, WarehouseId = whId });
        await db.SaveChangesAsync(ct);
        var whIds = await db.StoreWarehouses.Where(sw => sw.StoreId == s.Id).Select(sw => sw.WarehouseId).ToListAsync(ct);
        var warehouses = await db.Warehouses.AsNoTracking().Where(w => whIds.Contains(w.Id)).Select(w => new { id = w.Id, name = w.Name, code = w.Code }).ToListAsync(ct);
        return Results.Created("/api/v1/shop/stores/", new { id = s.Id, name = s.Name, code = s.Code, warehouses });
    }

    private static async Task<IResult> GetStore(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.Stores.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        var whIds = await db.StoreWarehouses.Where(sw => sw.StoreId == id).Select(sw => sw.WarehouseId).ToListAsync(ct);
        var warehouses = await db.Warehouses.AsNoTracking().Where(w => whIds.Contains(w.Id)).Select(w => new { id = w.Id, name = w.Name, code = w.Code }).ToListAsync(ct);
        return Results.Ok(new { id = s.Id, workspace = s.WorkspaceId, name = s.Name, code = s.Code, description = s.Description ?? "", is_active = s.IsActive, warehouses });
    }

    private static async Task<IResult> PatchStore(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.Stores.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<StorePatchBody>(ct);
        if (body?.name != null) s.Name = body.name;
        s.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = s.Id, name = s.Name, code = s.Code });
    }

    private static async Task<IResult> SetStoreWarehouses(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.Stores.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<StoreWarehousesBody>(ct);
        if (body?.warehouse_ids != null)
        {
            var existing = await db.StoreWarehouses.Where(sw => sw.StoreId == id).ToListAsync(ct);
            db.StoreWarehouses.RemoveRange(existing);
            foreach (var whId in body.warehouse_ids)
                db.StoreWarehouses.Add(new StoreWarehouse { StoreId = id, WarehouseId = whId });
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { success = true });
    }

    private static async Task<IResult> ListCarts(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Carts.AsNoTracking().Where(c => !wid.HasValue || c.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(c => c.UpdatedAt).Select(c => new { id = c.Id, workspace = c.WorkspaceId, customer = c.CustomerId, status = c.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetCart(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && (!wid.HasValue || c.WorkspaceId == wid.Value), ct);
        if (cart == null) return Results.NotFound();
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        var total = await db.CartItems.Where(i => i.CartId == id).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        return Results.Ok(new { id = cart.Id, workspace = cart.WorkspaceId, customer = cart.CustomerId, status = cart.Status, total = total.ToString("F2"), items });
    }

    private static async Task<IResult> CreateCart(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var c = new Cart { WorkspaceId = wid.Value, Status = "active", SessionKey = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Carts.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/shop/carts/", new { id = c.Id, workspace = c.WorkspaceId, customer = (int?)null, status = c.Status, total = "0.00", items = Array.Empty<object>() });
    }

    private static async Task<IResult> CartAddItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CartItemBody>(ct);
        if (body == null || body.product <= 0) return Results.BadRequest();
        var qty = body.quantity ?? 1;
        if (qty <= 0) return Results.BadRequest(new { detail = "Quantity must be greater than 0." });
        if (qty > 10000) return Results.BadRequest(new { detail = "Quantity cannot exceed 10000." });
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid.Value && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) { cart = new Cart { WorkspaceId = wid.Value, Status = "active", SessionKey = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }; db.Carts.Add(cart); await db.SaveChangesAsync(ct); }
        var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == body.product && p.WorkspaceId == wid.Value, ct);
        if (prod == null) return Results.NotFound();
        decimal unitPrice = prod.Price;
        int? variantId = body.variant;
        if (variantId.HasValue) { var v = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == variantId.Value && v.ProductId == body.product, ct); if (v != null) unitPrice = v.Price ?? prod.Price; }
        var existing = await db.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.ProductId == body.product && i.VariantId == variantId, ct);
        if (existing != null) { existing.Quantity += qty; existing.UpdatedAt = DateTime.UtcNow; }
        else db.CartItems.Add(new CartItem { CartId = cart.Id, ProductId = body.product, VariantId = variantId, Quantity = qty, UnitPrice = unitPrice, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        var total = await db.CartItems.Where(i => i.CartId == cart.Id).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == cart.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = cart.Id, workspace = cart.WorkspaceId, customer = cart.CustomerId, status = cart.Status, total = total.ToString("F2"), items });
    }

    private static async Task<IResult> CartRemoveItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<CartRemoveItemBody>(ct);
        if (body?.item_id <= 0) return Results.BadRequest();
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == body.item_id, ct);
        if (item == null) return Results.NotFound();
        var cartId = item.CartId;
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cartId, ct);
        var total = await db.CartItems.Where(i => i.CartId == cartId).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        var items = await db.CartItems.AsNoTracking().Where(i => i.CartId == cartId).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = (i.Quantity * i.UnitPrice).ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = cartId, workspace = cart?.WorkspaceId ?? 0, customer = cart?.CustomerId, status = cart?.Status ?? "active", total = total.ToString("F2"), items });
    }

    private static async Task<IResult> CartClear(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) return Results.Ok(new { id = 0, workspace = wid ?? 0, customer = (int?)null, status = "active", items = Array.Empty<object>(), total = "0.00" });
        var items = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = cart.Id, workspace = cart.WorkspaceId, customer = cart.CustomerId, status = cart.Status, items = Array.Empty<object>(), total = "0.00" });
    }

    private static async Task<IResult> CartCheckout(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CheckoutBody>(ct);
        if (body == null || body.store <= 0 || body.shipping_address <= 0) return Results.BadRequest(new { detail = "store and shipping_address are required." });
        var cart = await db.Carts.FirstOrDefaultAsync(c => c.WorkspaceId == wid.Value && (c.Status == "open" || c.Status == "active"), ct);
        if (cart == null) return Results.BadRequest(new { detail = "Cart is empty." });
        var cartItems = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        if (!cartItems.Any()) return Results.BadRequest(new { detail = "Cart is empty." });
        var store = await db.Stores.AsNoTracking().FirstOrDefaultAsync(s => s.Id == body.store && s.WorkspaceId == wid.Value, ct);
        if (store == null) return Results.NotFound(new { detail = "Store not found." });
        var customerId = cart.CustomerId ?? await db.Customers.AsNoTracking().Where(c => c.WorkspaceId == wid.Value).Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (customerId == 0) return Results.BadRequest(new { detail = "Customer required." });
        var orderNum = await OrderNumberService.GenerateAsync(ord => db.Orders.AnyAsync(o => o.OrderNumber == ord, ct));
        decimal subtotal = cartItems.Sum(it => it.Quantity * it.UnitPrice);
        decimal shipping = 0, tax = 0, discount = 0;
        var order = new Order
        {
            WorkspaceId = wid.Value,
            CustomerId = customerId,
            StoreId = body.store,
            OrderNumber = orderNum,
            Status = "pending",
            PaymentStatus = "pending",
            Subtotal = subtotal,
            ShippingCost = shipping,
            Tax = tax,
            Discount = discount,
            TotalAmount = subtotal + shipping + tax - discount,
            ShippingAddressId = body.shipping_address,
            BillingAddressId = body.billing_address ?? body.shipping_address,
            CustomerNote = body.customer_note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        foreach (var it in cartItems)
        {
            var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == it.ProductId, ct);
            var variant = it.VariantId.HasValue ? await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == it.VariantId, ct) : null;
            db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = it.ProductId,
                VariantId = it.VariantId,
                ProductName = prod?.Name ?? "",
                VariantName = variant?.Name ?? "",
                Sku = variant?.Sku ?? prod?.Sku ?? "",
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                TotalPrice = it.Quantity * it.UnitPrice,
                CreatedAt = DateTime.UtcNow
            });
        }
        db.CartItems.RemoveRange(cartItems);
        await db.SaveChangesAsync(ct);
        var orderItems = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == order.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Created("/api/v1/shop/orders/", new
        {
            id = order.Id,
            order_number = order.OrderNumber,
            workspace = order.WorkspaceId,
            customer_id = order.CustomerId,
            store_id = order.StoreId,
            status = order.Status,
            payment_status = order.PaymentStatus,
            subtotal_amount = order.Subtotal.ToString("F2"),
            shipping_amount = order.ShippingCost.ToString("F2"),
            discount_amount = order.Discount.ToString("F2"),
            total_amount = order.TotalAmount.ToString("F2"),
            shipping_address_id = order.ShippingAddressId,
            billing_address_id = order.BillingAddressId,
            items = orderItems,
            created_at = order.CreatedAt,
            updated_at = order.UpdatedAt
        });
    }

    private static async Task<IResult> ListOrders(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Orders.AsNoTracking().Where(o => !wid.HasValue || o.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var (page, pageSize) = Pagination.FromRequest(req);
        var list = await query.OrderByDescending(o => o.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, subtotal = o.Subtotal, total = o.TotalAmount, shipping_cost = o.ShippingCost, tax = o.Tax, discount = o.Discount }).ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateOrder(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<OrderCreateBody>(ct);
        if (body == null || body.customer_id <= 0 || body.store_id <= 0 || body.shipping_address_id <= 0) return Results.BadRequest(new { detail = "customer_id, store_id, shipping_address_id are required." });
        var orderNum = await OrderNumberService.GenerateAsync(ord => db.Orders.AnyAsync(o => o.OrderNumber == ord, ct));
        var order = new Order
        {
            WorkspaceId = wid.Value,
            CustomerId = body.customer_id,
            StoreId = body.store_id,
            OrderNumber = orderNum,
            Status = body.status ?? "pending",
            PaymentStatus = body.payment_status ?? "pending",
            Subtotal = 0,
            ShippingCost = 0,
            Tax = 0,
            Discount = 0,
            TotalAmount = 0,
            ShippingAddressId = body.shipping_address_id,
            BillingAddressId = body.billing_address_id ?? body.shipping_address_id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == order.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Created("/api/v1/shop/orders/", new { id = order.Id, order_number = order.OrderNumber, workspace = order.WorkspaceId, customer_id = order.CustomerId, store_id = order.StoreId, status = order.Status, payment_status = order.PaymentStatus, subtotal_amount = order.Subtotal.ToString("F2"), shipping_amount = order.ShippingCost.ToString("F2"), discount_amount = order.Discount.ToString("F2"), total_amount = order.TotalAmount.ToString("F2"), shipping_address_id = order.ShippingAddressId, billing_address_id = order.BillingAddressId, items, created_at = order.CreatedAt, updated_at = order.UpdatedAt });
    }

    private static async Task<IResult> GetOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, workspace = o.WorkspaceId, customer_id = o.CustomerId, store_id = o.StoreId, status = o.Status, payment_status = o.PaymentStatus, subtotal_amount = o.Subtotal.ToString("F2"), shipping_amount = o.ShippingCost.ToString("F2"), discount_amount = o.Discount.ToString("F2"), total_amount = o.TotalAmount.ToString("F2"), shipping_address_id = o.ShippingAddressId, billing_address_id = o.BillingAddressId, items, created_at = o.CreatedAt, updated_at = o.UpdatedAt, packages = new List<object>() });
    }

    private static async Task<IResult> PatchOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<OrderPatchBody>(ct);
        if (body?.status != null) o.Status = body.status;
        if (body?.payment_status != null) o.PaymentStatus = body.payment_status;
        if (body?.shipping_address_id.HasValue == true) o.ShippingAddressId = body.shipping_address_id;
        if (body?.billing_address_id.HasValue == true) o.BillingAddressId = body.billing_address_id;
        o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, subtotal_amount = o.Subtotal.ToString("F2"), shipping_amount = o.ShippingCost.ToString("F2"), total_amount = o.TotalAmount.ToString("F2"), items });
    }

    private static async Task<IResult> OrderUpdateStatus(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<OrderUpdateStatusBody>(ct);
        if (body?.status == null) return Results.BadRequest(new { detail = "Status is required." });
        o.Status = body.status;
        o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, items });
    }

    private static async Task<IResult> OrderCancel(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        if (o.Status == "delivered" || o.Status == "cancelled" || o.Status == "refunded")
            return Results.BadRequest(new { detail = $"Order in '{o.Status}' status cannot be cancelled." });
        o.Status = "cancelled";
        o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, items });
    }

    private static async Task<IResult> OrderUpdateItems(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (order == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<OrderUpdateItemsBody>(ct);
        if (body?.items == null || !body.items.Any()) return Results.BadRequest(new { detail = "Items are required." });
        var existing = await db.OrderItems.Where(i => i.OrderId == id).ToListAsync(ct);
        db.OrderItems.RemoveRange(existing);
        decimal subtotal = 0;
        foreach (var it in body.items)
        {
            if (it.product <= 0 || (it.quantity ?? 0) <= 0) continue;
            var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == it.product && p.WorkspaceId == wid, ct);
            if (prod == null) continue;
            decimal unitPrice = prod.Price;
            Variant? variant = null;
            if (it.variant.HasValue && it.variant.Value > 0)
            {
                variant = await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == it.variant && v.ProductId == it.product, ct);
                if (variant != null && variant.Price.HasValue) unitPrice = variant.Price.Value;
            }
            var qty = Math.Clamp(it.quantity ?? 1, 1, 10000);
            var lineTotal = qty * unitPrice;
            subtotal += lineTotal;
            db.OrderItems.Add(new OrderItem
            {
                OrderId = id,
                ProductId = it.product,
                VariantId = variant?.Id,
                ProductName = prod.Name,
                VariantName = variant?.Name ?? "",
                Sku = variant?.Sku ?? prod.Sku,
                Quantity = qty,
                UnitPrice = unitPrice,
                TotalPrice = lineTotal,
                CreatedAt = DateTime.UtcNow
            });
        }
        order.Subtotal = subtotal;
        order.TotalAmount = subtotal + order.ShippingCost + order.Tax - order.Discount;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = order.Id, order_number = order.OrderNumber, status = order.Status, subtotal_amount = order.Subtotal.ToString("F2"), total_amount = order.TotalAmount.ToString("F2"), items });
    }

    private sealed record CategoryCreateBody(string? name, string? slug, string? language, int? parent);
    private sealed record CategoryPatchBody(string? name);
    private sealed record ProductCreateBody(string? name, string? slug, string? sku, string? price, string? description, string? short_description, string? compare_at_price, string? language, bool? track_inventory, int? stock_quantity, bool? is_active, bool? is_featured, List<int>? category_ids);
    private sealed record ProductPatchBody(string? name, string? slug, string? sku, string? description, string? short_description, string? price, string? compare_at_price, bool? is_active, string? language, List<int>? category_ids);
    private sealed record VariantCreateBody(int product, string? sku, string? name, string? price, Dictionary<string, string>? options, int? stock_quantity);
    private sealed record VariantPatchBody(string? name);
    private sealed record StoreCreateBody(string? name, string? code, string? description, List<int>? warehouse_ids);
    private sealed record StorePatchBody(string? name);
    private sealed record StoreWarehousesBody(List<int>? warehouse_ids);
    private sealed record CartItemBody(int product, int? variant, int? quantity);
    private sealed record CartRemoveItemBody(int item_id);
    private sealed record CheckoutBody(int store, int shipping_address, int? billing_address, string? customer_note);
    private sealed record OrderCreateBody(int customer_id, int store_id, int shipping_address_id, int? billing_address_id, string? status, string? payment_status);
    private sealed record OrderPatchBody(string? status, string? payment_status, int? shipping_address_id, int? billing_address_id);
    private sealed record OrderUpdateStatusBody(string? status);
    private sealed record OrderUpdateItemsBody(List<OrderItemInput>? items);
    private sealed record OrderItemCreateBody(int order, int product, int? variant, int quantity, decimal unit_price);

    private static async Task<IResult> OrderProcess(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        o.Status = "processing"; o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status });
    }

    private static async Task<IResult> OrderShip(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        o.Status = "shipped"; o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status });
    }

    private static async Task<IResult> OrderComplete(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        o.Status = "completed"; o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status });
    }

    private static async Task<IResult> ListOrderItems(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        // Filter via join to Orders to respect workspace
        var orderIds = wid.HasValue
            ? await db.Orders.AsNoTracking().Where(o => o.WorkspaceId == wid.Value).Select(o => o.Id).ToListAsync(ct)
            : null;
        var query = db.OrderItems.AsNoTracking();
        if (orderIds != null) query = query.Where(i => orderIds.Contains(i.OrderId));
        var list = await query.Select(i => new { id = i.Id, order = i.OrderId, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { count = list.Count, next = (string?)null, previous = (string?)null, results = list });
    }

    private static async Task<IResult> CreateOrderItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<OrderItemCreateBody>(ct);
        if (body == null || body.order == 0) return Results.BadRequest();
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == body.order && o.WorkspaceId == wid.Value, ct);
        if (order == null) return Results.NotFound();
        var unitPrice = body.unit_price > 0 ? body.unit_price : (await db.Products.Where(p => p.Id == body.product).Select(p => p.Price).FirstOrDefaultAsync(ct));
        var totalPrice = unitPrice * body.quantity;
        var item = new Bfg.Core.Shop.OrderItem
        {
            OrderId = body.order, ProductId = body.product,
            VariantId = body.variant, Quantity = body.quantity,
            UnitPrice = unitPrice, TotalPrice = totalPrice, CreatedAt = DateTime.UtcNow
        };
        db.OrderItems.Add(item);
        order.Subtotal += totalPrice;
        order.TotalAmount = order.Subtotal + order.ShippingCost - order.Discount;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Created($"/api/v1/shop/order-items/", new { id = item.Id, order = item.OrderId, product = item.ProductId, variant = item.VariantId, quantity = item.Quantity, unit_price = item.UnitPrice.ToString("F2"), total_price = item.TotalPrice.ToString("F2") });
    }

    private static async Task<IResult> GetOrderItem(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var item = await db.OrderItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id, ct);
        if (item == null) return Results.NotFound();
        return Results.Ok(new { id = item.Id, order = item.OrderId, product = item.ProductId, variant = item.VariantId, quantity = item.Quantity, unit_price = item.UnitPrice.ToString("F2"), total_price = item.TotalPrice.ToString("F2") });
    }
    private sealed record OrderItemInput(int product, int? variant, int? quantity);
}
