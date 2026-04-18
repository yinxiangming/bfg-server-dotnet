using System.Text.Json;
using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using Bfg.Core.Common;
using Bfg.Core.Delivery;
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
        group.MapPost("/products/tags/", CreateProductTag);
        group.MapGet("/products/{id:int}", GetProduct);
        group.MapPatch("/products/{id:int}", PatchProduct);
        group.MapDelete("/products/{id:int}", DeleteProduct);

        group.MapGet("/variants", ListVariants);
        group.MapPost("/variants/", CreateVariant);
        group.MapGet("/variants/{id:int}", GetVariant);
        group.MapPatch("/variants/{id:int}", PatchVariant);

        group.MapPost("/product-media/", CreateProductMedia);

        group.MapGet("/sales-channels", ListSalesChannels);
        group.MapPost("/sales-channels/", CreateSalesChannel);
        group.MapPost("/sales-channels/{id:int}/add_product/", SalesChannelAddProduct);

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

        group.MapGet("/orders/", ListOrders);
        group.MapPost("/orders/", CreateOrder);
        group.MapGet("/orders/{id:int}/", GetOrder);
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

        group.MapPost("/order-packages/", CreateOrderPackage);
        group.MapPost("/order-packages/calculate_shipping/", CalculateOrderPackagesShipping);
        group.MapPost("/order-packages/update_order_shipping/", UpdateOrderShippingFromPackages);
        group.MapGet("/returns/", () => Results.Ok(Array.Empty<object>()));
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
        var c = new ProductCategory
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Slug = body.slug ?? "",
            Description = "",
            Language = body.language ?? "en",
            ParentId = body.parent,
            IsActive = true,
            SortOrder = 100
        };
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
            .Select(p => new { id = p.Id, workspace = p.WorkspaceId, name = p.Name, slug = p.Slug, sku = p.Sku, description = p.Description, short_description = p.ShortDescription, price = p.Price.ToString("F2"), compare_at_price = p.ComparePrice.HasValue ? p.ComparePrice.Value.ToString("F2") : (string?)null, is_active = p.IsActive, track_inventory = p.TrackInventory, stock_quantity = p.StockQuantity, language = p.Language, created_at = p.CreatedAt }).ToListAsync(ct);
        var result = new List<object>();
        foreach (var p in list)
        {
            var categoryIds = await db.ProductCategoryProducts.Where(pcp => pcp.ProductId == p.id).Select(pcp => pcp.ProductCategoryId).ToListAsync(ct);
            result.Add(new { p.id, p.workspace, p.name, p.slug, p.sku, p.description, p.short_description, p.price, p.compare_at_price, category_ids = categoryIds, p.is_active, p.track_inventory, p.stock_quantity, p.language, p.created_at });
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
            Barcode = "",
            Description = body.description ?? "",
            ShortDescription = body.short_description ?? "",
            Price = price,
            ComparePrice = decimal.TryParse(body.compare_at_price, out var cp) ? cp : (decimal?)null,
            Language = language,
            TrackInventory = body.track_inventory ?? true,
            StockQuantity = body.stock_quantity ?? 0,
            LowStockThreshold = 0,
            RequiresShipping = true,
            IsSubscription = false,
            MetaTitle = "",
            MetaDescription = "",
            Condition = "new",
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
        if (body.tag_ids is { Count: > 0 })
        {
            foreach (var tid in body.tag_ids)
                db.ProductTagProducts.Add(new ProductTagProduct { ProductId = prod.Id, ProductTagId = tid });
            await db.SaveChangesAsync(ct);
        }
        return Results.Created("/api/v1/shop/products/", new { id = prod.Id, workspace = prod.WorkspaceId, name = prod.Name, slug = prod.Slug, sku = prod.Sku, description = prod.Description, short_description = prod.ShortDescription, price = prod.Price.ToString("F2"), compare_at_price = prod.ComparePrice?.ToString("F2"), category_ids = body.category_ids ?? new List<int>(), is_active = prod.IsActive, track_inventory = prod.TrackInventory, stock_quantity = prod.StockQuantity, language = prod.Language, created_at = prod.CreatedAt });
    }

    private static async Task<IResult> CreateProductTag(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<ProductTagCreateBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.slug)) return Results.BadRequest();
        var t = new ProductTag { WorkspaceId = wid.Value, Name = body.name ?? "", Slug = body.slug, Language = body.language ?? "en" };
        db.ProductTags.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/shop/products/tags/", new { id = t.Id, name = t.Name, slug = t.Slug, language = t.Language });
    }

    private static async Task<IResult> GetProduct(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var categoryIds = await db.ProductCategoryProducts.Where(pcp => pcp.ProductId == id).Select(pcp => pcp.ProductCategoryId).ToListAsync(ct);
        var variants = await db.Variants.AsNoTracking().Where(v => v.ProductId == id).OrderBy(v => v.SortOrder).Select(v => new { id = v.Id, sku = v.Sku, name = v.Name, price = v.Price.HasValue ? v.Price.Value.ToString("F2") : (string?)null, stock_quantity = v.StockQuantity, is_active = v.IsActive }).ToListAsync(ct);
        return Results.Ok(new { id = p.Id, workspace = p.WorkspaceId, name = p.Name, slug = p.Slug, sku = p.Sku, description = p.Description, short_description = p.ShortDescription, price = p.Price.ToString("F2"), compare_at_price = p.ComparePrice?.ToString("F2"), category_ids = categoryIds, is_active = p.IsActive, track_inventory = p.TrackInventory, stock_quantity = p.StockQuantity, language = p.Language, created_at = p.CreatedAt, variants });
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
        return Results.Ok(new { id = p.Id, workspace = p.WorkspaceId, name = p.Name, slug = p.Slug, sku = p.Sku, description = p.Description, short_description = p.ShortDescription, price = p.Price.ToString("F2"), compare_at_price = p.ComparePrice?.ToString("F2"), category_ids = categoryIds, is_active = p.IsActive, track_inventory = p.TrackInventory, stock_quantity = p.StockQuantity, language = p.Language, created_at = p.CreatedAt });
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
        var raw = await query.OrderBy(v => v.SortOrder).Select(v => new { v.Id, v.ProductId, v.Sku, v.Name, v.Price, v.Options, v.StockQuantity, v.IsActive }).ToListAsync(ct);
        var list = raw.Select(v => (object)new { id = v.Id, product = v.ProductId, sku = v.Sku, name = v.Name, price = v.Price.HasValue ? v.Price.Value.ToString("F2") : (string?)null, options = v.Options, stock_quantity = v.StockQuantity, is_active = v.IsActive }).ToList();
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateVariant(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<VariantCreateRequest>(ct);
        if (body == null || body.Product <= 0) return Results.BadRequest();
        var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == body.Product && p.WorkspaceId == wid.Value, ct);
        if (prod == null) return Results.NotFound();
        var price = body.Price != null && decimal.TryParse(body.Price, out var pv) ? pv : (decimal?)null;
        var v = new Variant
        {
            ProductId = body.Product,
            Sku = body.Sku ?? "",
            Name = body.Name ?? "",
            Options = SerializeVariantOptionsJson(body.Options),
            Price = price ?? prod.Price,
            StockQuantity = body.StockQuantity ?? 0,
            IsActive = true,
            SortOrder = 100
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
        if (body != null)
        {
            if (body.name != null) v.Name = body.name;
            if (body.sku != null) v.Sku = body.sku;
            if (body.price != null && decimal.TryParse(body.price, out var pv)) v.Price = pv;
            if (body.stock_quantity.HasValue) v.StockQuantity = body.stock_quantity.Value;
            if (body.is_active.HasValue) v.IsActive = body.is_active.Value;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = v.Id, product = v.ProductId, sku = v.Sku, name = v.Name, price = v.Price.HasValue ? v.Price.Value.ToString("F2") : (string?)null, stock_quantity = v.StockQuantity, is_active = v.IsActive });
    }

    private static async Task<IResult> ListSalesChannels(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.SalesChannels.AsNoTracking();
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.OrderBy(s => s.Name)
            .Select(s => new
            {
                id = s.Id,
                name = s.Name,
                code = s.Code,
                channel_type = s.ChannelType,
                is_active = s.IsActive,
                is_default = s.IsDefault
            })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateSalesChannel(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<SalesChannelCreateBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.name) || string.IsNullOrWhiteSpace(body.code))
            return Results.BadRequest();
        var now = DateTime.UtcNow;
        var ch = new SalesChannel
        {
            WorkspaceId = wid.Value,
            Name = body.name,
            Code = body.code,
            ChannelType = string.IsNullOrWhiteSpace(body.channel_type) ? "custom" : body.channel_type!,
            Description = body.description ?? "",
            IsActive = body.is_active ?? true,
            IsDefault = body.is_default ?? false,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.SalesChannels.Add(ch);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/shop/sales-channels/", new
        {
            id = ch.Id,
            name = ch.Name,
            code = ch.Code,
            channel_type = ch.ChannelType,
            is_active = ch.IsActive,
            is_default = ch.IsDefault
        });
    }

    private static async Task<IResult> SalesChannelAddProduct(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<SalesChannelAddProductBody>(ct);
        if (body?.product_id is not int pid || pid <= 0)
            return Results.BadRequest(new { detail = "product_id is required" });
        var channel = await db.SalesChannels.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.WorkspaceId == wid.Value, ct);
        if (channel == null) return Results.NotFound();
        var product = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pid && p.WorkspaceId == wid.Value, ct);
        if (product == null) return Results.NotFound(new { detail = "Product not found" });
        var existing = await db.ProductChannelListings
            .FirstOrDefaultAsync(l => l.ChannelId == id && l.ProductId == pid, ct);
        if (existing != null)
            return Results.Ok(new { success = true, created = false, product_id = product.Id, product_name = product.Name });
        db.ProductChannelListings.Add(new ProductChannelListing
        {
            ChannelId = id,
            ProductId = pid,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { success = true, created = true, product_id = product.Id, product_name = product.Name });
    }

    private static async Task<IResult> CreateProductMedia(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        if (ctx.Request.ContentType == null || !ctx.Request.ContentType.Contains("multipart", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest(new { detail = "multipart required" });
        var form = ctx.Request.Form;
        if (!int.TryParse(form["product"].ToString(), out var productId) || productId <= 0)
            return Results.BadRequest(new { detail = "product is required" });
        var product = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.WorkspaceId == wid.Value, ct);
        if (product == null) return Results.NotFound(new { detail = "Product not found" });
        var contentTypeId = await db.DjangoContentTypes.AsNoTracking()
            .Where(x => x.AppLabel == "shop" && x.Model == "product")
            .Select(x => x.Id)
            .FirstOrDefaultAsync(ct);
        if (contentTypeId == 0)
            return Results.Json(new { detail = "Content type shop.product missing in django_content_type" }, statusCode: 500);
        var upload = form.Files.GetFile("file");
        var mediaType = form["media_type"].ToString();
        if (string.IsNullOrEmpty(mediaType)) mediaType = "image";
        var altText = form["alt_text"].ToString() ?? "";
        var position = int.TryParse(form["position"].ToString(), out var pos) ? pos : 100;
        var now = DateTime.UtcNow;
        var storedName = upload != null
            ? $"media/{wid.Value}/products/{Guid.NewGuid():N}_{upload.FileName}"
            : $"media/{wid.Value}/products/{Guid.NewGuid():N}.bin";
        var media = new Media
        {
            WorkspaceId = wid.Value,
            File = storedName,
            MediaType = mediaType,
            AltText = altText,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Media.Add(media);
        await db.SaveChangesAsync(ct);
        var link = await db.MediaLinks.FirstOrDefaultAsync(
            l => l.MediaId == media.Id && l.ContentTypeId == contentTypeId && l.ObjectId == productId,
            ct);
        if (link == null)
        {
            link = new MediaLink
            {
                MediaId = media.Id,
                ContentTypeId = contentTypeId,
                ObjectId = productId,
                Position = position,
                Description = "",
                CreatedAt = now,
                UpdatedAt = now
            };
            db.MediaLinks.Add(link);
        }
        else
        {
            link.Position = position;
            link.UpdatedAt = now;
        }
        await db.SaveChangesAsync(ct);
        var filePublic = $"/{storedName}";
        return Results.Created("/api/v1/shop/product-media/", new
        {
            id = link.Id,
            media_id = media.Id,
            media_type = media.MediaType,
            file = filePublic,
            alt_text = media.AltText,
            position = link.Position,
            description = link.Description
        });
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
        var s = new Store { WorkspaceId = wid.Value, Name = body.name ?? "", Code = body.code ?? "", Description = body.description ?? "", Settings = "{}", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
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

    private static async Task<IResult> ListCarts(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await carts.ListSummariesAsync(wid, ct);
        return Results.Ok(list.Select(CartJson.ListRow).ToList());
    }

    private static async Task<IResult> GetCart(CartService carts, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var detail = await carts.GetDetailAsync(id, wid, ct);
        if (detail == null) return Results.NotFound();
        return Results.Ok(CartJson.Detail(detail));
    }

    private static async Task<IResult> CreateCart(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var d = await carts.CreateAsync(wid.Value, ct);
        return Results.Created("/api/v1/shop/carts/", CartJson.Detail(d));
    }

    private static async Task<IResult> CartAddItem(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        JsonDocument doc;
        try
        {
            doc = await JsonDocument.ParseAsync(ctx.Request.Body, cancellationToken: ct);
        }
        catch (JsonException)
        {
            return Results.BadRequest(new { detail = "Invalid JSON body." });
        }
        using (doc)
        {
        var root = doc.RootElement;
        if (!root.TryGetProperty("product", out var pEl) || pEl.ValueKind != JsonValueKind.Number || !pEl.TryGetInt32(out var productId) || productId <= 0)
            return Results.BadRequest(new { detail = "product is required." });
        int? variantId = null;
        if (root.TryGetProperty("variant", out var vEl) && vEl.ValueKind == JsonValueKind.Number && vEl.TryGetInt32(out var vid) && vid > 0)
            variantId = vid;
        int qty;
        if (!root.TryGetProperty("quantity", out var qEl))
            qty = 1;
        else if (qEl.ValueKind == JsonValueKind.Number && qEl.TryGetInt32(out qty))
        {
            // ok
        }
        else
            return Results.BadRequest(new { detail = "Quantity must be a valid integer." });

        var r = await carts.AddItemAsync(wid.Value, productId, variantId, qty, new CartAddConstraints(1, 10000), ct);
        if (!r.Success)
            return r.ErrorCode == "not_found" ? Results.NotFound() : Results.BadRequest(new { detail = r.ErrorMessage });
        return Results.Ok(CartJson.Detail(r.Detail!));
        }
    }

    private static async Task<IResult> CartRemoveItem(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<CartRemoveItemBody>(ct);
        if (body is not { item_id: > 0 }) return Results.BadRequest();
        var r = await carts.RemoveLineAsync(body.item_id, ct);
        if (!r.Success) return Results.NotFound();
        return Results.Ok(CartJson.Detail(r.Detail!));
    }

    private static async Task<IResult> CartClear(CartService carts, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var d = await carts.ClearCurrentCartAsync(wid.Value, ct);
        return Results.Ok(CartJson.ClearedCart(d));
    }

    private static async Task<IResult> CartCheckout(OrderCheckoutService checkout, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CheckoutBody>(ct);
        if (body == null || body.store <= 0 || body.shipping_address <= 0)
            return Results.BadRequest(new { detail = "store and shipping_address are required." });
        var uid = WorkspaceMiddleware.GetCurrentUserId(ctx);
        var r = await checkout.CreateFromCurrentCartAsync(
            wid.Value,
            preferredCartId: null,
            authenticatedUserId: uid,
            body.store,
            body.shipping_address,
            body.billing_address,
            body.customer_note,
            body.coupon_code,
            body.gift_card_code,
            validateStoreInWorkspace: true,
            storefrontSessionKey: null,
            ct);
        if (!r.Success)
        {
            if (r.ErrorCode == "store_not_found")
                return Results.NotFound(new { detail = r.ErrorMessage });
            return Results.BadRequest(new { detail = r.ErrorMessage });
        }

        return Results.Created("/api/v1/shop/orders/", OrderCheckoutJson.CreatedBody(r.Payload!));
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
        if (!await db.Customers.AsNoTracking().AnyAsync(c => c.Id == body.customer_id && c.WorkspaceId == wid.Value, ct))
            return Results.BadRequest(new { detail = "customer_id is not in this workspace." });
        if (!await db.Stores.AsNoTracking().AnyAsync(s => s.Id == body.store_id && s.WorkspaceId == wid.Value, ct))
            return Results.BadRequest(new { detail = "store_id is not in this workspace." });
        var billingId = body.billing_address_id ?? body.shipping_address_id;
        if (!await db.Addresses.AsNoTracking().AnyAsync(a => a.Id == body.shipping_address_id && a.WorkspaceId == wid.Value, ct))
            return Results.BadRequest(new { detail = "shipping_address_id is not in this workspace." });
        if (!await db.Addresses.AsNoTracking().AnyAsync(a => a.Id == billingId && a.WorkspaceId == wid.Value, ct))
            return Results.BadRequest(new { detail = "billing_address_id is not in this workspace." });
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
            CustomerNote = "",
            AdminNote = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == order.Id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Created("/api/v1/shop/orders/", new { id = order.Id, order_number = order.OrderNumber, workspace = order.WorkspaceId, customer_id = order.CustomerId, store_id = order.StoreId, status = order.Status, payment_status = order.PaymentStatus, subtotal = order.Subtotal.ToString("F2"), shipping_cost = order.ShippingCost.ToString("F2"), tax = order.Tax.ToString("F2"), discount = order.Discount.ToString("F2"), total = order.TotalAmount.ToString("F2"), subtotal_amount = order.Subtotal.ToString("F2"), shipping_amount = order.ShippingCost.ToString("F2"), discount_amount = order.Discount.ToString("F2"), total_amount = order.TotalAmount.ToString("F2"), shipping_address_id = order.ShippingAddressId, billing_address_id = order.BillingAddressId, items, created_at = order.CreatedAt, updated_at = order.UpdatedAt });
    }

    private static async Task<IResult> GetOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var o = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (o == null) return Results.NotFound();
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        var packages = await FormatOrderPackagesAsync(db, id, ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, workspace = o.WorkspaceId, customer_id = o.CustomerId, store_id = o.StoreId, status = o.Status, payment_status = o.PaymentStatus, subtotal = o.Subtotal.ToString("F2"), shipping_cost = o.ShippingCost.ToString("F2"), tax = o.Tax.ToString("F2"), discount = o.Discount.ToString("F2"), total = o.TotalAmount.ToString("F2"), subtotal_amount = o.Subtotal.ToString("F2"), shipping_amount = o.ShippingCost.ToString("F2"), discount_amount = o.Discount.ToString("F2"), total_amount = o.TotalAmount.ToString("F2"), shipping_address_id = o.ShippingAddressId, billing_address_id = o.BillingAddressId, items, created_at = o.CreatedAt, updated_at = o.UpdatedAt, packages });
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
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, subtotal = o.Subtotal.ToString("F2"), shipping_cost = o.ShippingCost.ToString("F2"), tax = o.Tax.ToString("F2"), discount = o.Discount.ToString("F2"), total = o.TotalAmount.ToString("F2"), subtotal_amount = o.Subtotal.ToString("F2"), shipping_amount = o.ShippingCost.ToString("F2"), discount_amount = o.Discount.ToString("F2"), total_amount = o.TotalAmount.ToString("F2"), items });
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
                TotalPrice = lineTotal
            });
        }
        order.Subtotal = subtotal;
        order.TotalAmount = subtotal + order.ShippingCost + order.Tax - order.Discount;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = order.Id, order_number = order.OrderNumber, status = order.Status, subtotal_amount = order.Subtotal.ToString("F2"), total_amount = order.TotalAmount.ToString("F2"), items });
    }

    private static async Task<List<object>> FormatOrderPackagesAsync(BfgDbContext db, int orderId, CancellationToken ct)
    {
        var rows = await db.DeliveryPackages.AsNoTracking()
            .Where(p => p.OrderId == orderId)
            .OrderBy(p => p.Id)
            .Select(p => new { p.Id, p.OrderId, p.TemplateId, p.Weight, p.PackageNumber, p.StatusId })
            .ToListAsync(ct);
        return rows.Select(p => (object)new
        {
            id = p.Id,
            order = p.OrderId,
            order_id = p.OrderId,
            template = p.TemplateId,
            template_id = p.TemplateId,
            weight = p.Weight?.ToString("F2"),
            package_number = p.PackageNumber,
            freight_status = p.StatusId
        }).ToList();
    }

    private static async Task<IResult> CreateOrderPackage(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<OrderPackageCreateBody>(ct);
        if (body is not { Order: > 0, FreightStatus: > 0 }) return Results.BadRequest(new { detail = "order and freight_status are required." });
        var st = await db.FreightStatuses.AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == body.FreightStatus && f.WorkspaceId == wid.Value, ct);
        if (st == null) return Results.BadRequest(new { detail = "Invalid freight_status." });
        var ord = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == body.Order && o.WorkspaceId == wid.Value, ct);
        if (ord == null) return Results.NotFound();
        PackageTemplate? tplEntity = null;
        if (body.Template is int templateId && templateId > 0)
        {
            tplEntity = await db.PackageTemplates.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == templateId && t.WorkspaceId == wid.Value, ct);
            if (tplEntity == null) return Results.BadRequest(new { detail = "Invalid template." });
        }

        var len = body.Length ?? tplEntity?.Length;
        var pkgWidth = body.Width ?? tplEntity?.Width;
        var pkgHeight = body.Height ?? tplEntity?.Height;
        var vol = (len ?? 0) * (pkgWidth ?? 0) * (pkgHeight ?? 0);
        var volumetricW = vol > 0 ? vol / 5000m : (decimal?)null;
        var pkg = new DeliveryPackage
        {
            PackageNumber = $"PKG-{body.Order}-{Guid.NewGuid().ToString("N")[..8]}",
            Weight = body.Weight,
            Length = len,
            Width = pkgWidth,
            Height = pkgHeight,
            Pieces = body.Pieces ?? 1,
            State = string.IsNullOrEmpty(st.State) ? "PACKAGE" : st.State,
            Description = body.Description ?? "",
            Notes = "",
            CreatedAt = DateTime.UtcNow,
            OrderId = body.Order,
            StatusId = body.FreightStatus,
            TemplateId = body.Template
        };
        db.DeliveryPackages.Add(pkg);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/shop/order-packages/", new
        {
            id = pkg.Id,
            order = body.Order,
            order_id = body.Order,
            template = body.Template,
            template_id = body.Template,
            weight = pkg.Weight?.ToString("F2"),
            billing_weight = pkg.Weight?.ToString("F2"),
            volumetric_weight = volumetricW?.ToString("F2")
        });
    }

    private static async Task<IResult> CalculateOrderPackagesShipping(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<OrderShippingCalcBody>(ct);
        if (body is not { Order: > 0, FreightServiceId: > 0 }) return Results.BadRequest();
        var svc = await db.FreightServices.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == body.FreightServiceId && s.WorkspaceId == wid.Value, ct);
        if (svc == null) return Results.BadRequest();
        var pkgs = await db.DeliveryPackages.AsNoTracking().Where(p => p.OrderId == body.Order).ToListAsync(ct);
        var totalWeight = pkgs.Sum(p => p.Weight ?? 0);
        var shipping = svc.BasePrice + svc.PricePerKg * totalWeight;
        return Results.Ok(new
        {
            total_packages = pkgs.Count,
            total_billing_weight = totalWeight.ToString("F2"),
            shipping_cost = shipping.ToString("F2")
        });
    }

    private static async Task<IResult> UpdateOrderShippingFromPackages(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<OrderShippingCalcBody>(ct);
        if (body is not { Order: > 0, FreightServiceId: > 0 }) return Results.BadRequest();
        var svc = await db.FreightServices.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == body.FreightServiceId && s.WorkspaceId == wid.Value, ct);
        if (svc == null) return Results.BadRequest();
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == body.Order && o.WorkspaceId == wid.Value, ct);
        if (order == null) return Results.NotFound();
        var pkgs = await db.DeliveryPackages.AsNoTracking().Where(p => p.OrderId == body.Order).ToListAsync(ct);
        var totalWeight = pkgs.Sum(p => p.Weight ?? 0);
        var shipping = svc.BasePrice + svc.PricePerKg * totalWeight;
        order.ShippingCost = shipping;
        order.TotalAmount = order.Subtotal + order.ShippingCost + order.Tax - order.Discount;
        order.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { order_id = body.Order, shipping_cost = shipping.ToString("F2") });
    }

    private sealed record OrderPackageCreateBody(int Order, int FreightStatus, int? Template, decimal? Length, decimal? Width, decimal? Height, decimal? Weight, int? Pieces, string? Description);
    private sealed record OrderShippingCalcBody(int Order, int FreightServiceId);

    private sealed record CategoryCreateBody(string? name, string? slug, string? language, int? parent);
    private sealed record CategoryPatchBody(string? name);
    private sealed record ProductCreateBody(string? name, string? slug, string? sku, string? price, string? description, string? short_description, string? compare_at_price, string? language, bool? track_inventory, int? stock_quantity, bool? is_active, bool? is_featured, List<int>? category_ids, List<int>? tag_ids);
    private sealed record ProductTagCreateBody(string? name, string? slug, string? language);
    private sealed record ProductPatchBody(string? name, string? slug, string? sku, string? description, string? short_description, string? price, string? compare_at_price, bool? is_active, string? language, List<int>? category_ids);
    private sealed class VariantCreateRequest
    {
        public int Product { get; set; }
        public string? Sku { get; set; }
        public string? Name { get; set; }
        public string? Price { get; set; }
        public JsonElement Options { get; set; }
        public int? StockQuantity { get; set; }
    }

    private static string SerializeVariantOptionsJson(JsonElement options)
    {
        if (options.ValueKind != JsonValueKind.Object)
            return "{}";
        try
        {
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(options.GetRawText());
            return dict is { Count: > 0 } ? JsonSerializer.Serialize(dict) : "{}";
        }
        catch (JsonException)
        {
            return "{}";
        }
    }
    private sealed record VariantPatchBody(string? name, string? sku, string? price, int? stock_quantity, bool? is_active);
    private sealed record SalesChannelCreateBody(string? name, string? code, string? channel_type, string? description, bool? is_active, bool? is_default);
    private sealed record SalesChannelAddProductBody(int? product_id);
    private sealed record StoreCreateBody(string? name, string? code, string? description, List<int>? warehouse_ids);
    private sealed record StorePatchBody(string? name);
    private sealed record StoreWarehousesBody(List<int>? warehouse_ids);
    private sealed record CartItemBody(int product, int? variant, int? quantity);
    private sealed record CartRemoveItemBody(int item_id);
    private sealed class CheckoutBody
    {
        public int store { get; set; }
        public int shipping_address { get; set; }
        public int? billing_address { get; set; }
        public string? customer_note { get; set; }
        public string? coupon_code { get; set; }
        public string? gift_card_code { get; set; }
    }
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
            UnitPrice = unitPrice, TotalPrice = totalPrice
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
