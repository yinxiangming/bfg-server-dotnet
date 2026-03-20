using System.Text.Json;
using Bfg.Core;
using Bfg.Core.Shop;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Services;

/// <summary>
/// Cart domain operations shared by shop admin and storefront APIs.
/// </summary>
public sealed class CartService(BfgDbContext db)
{
    public const string StatusOpen = "open";

    public async Task<IReadOnlyList<CartListRow>> ListSummariesAsync(int? workspaceFilter, CancellationToken ct)
    {
        var query = db.Carts.AsNoTracking().Where(c => !workspaceFilter.HasValue || c.WorkspaceId == workspaceFilter.Value);
        var rows = await query.OrderByDescending(c => c.UpdatedAt).Select(c => new { c.Id, c.WorkspaceId, c.CustomerId }).ToListAsync(ct);
        return rows.Select(c => new CartListRow(c.Id, c.WorkspaceId, c.CustomerId, StatusOpen)).ToList();
    }

    public async Task<CartDetail?> GetDetailAsync(int cartId, int? workspaceFilter, CancellationToken ct)
    {
        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(
            c => c.Id == cartId && (!workspaceFilter.HasValue || c.WorkspaceId == workspaceFilter.Value), ct);
        if (cart == null) return null;
        return await BuildDetailAsync(cart, ct);
    }

    /// <summary>Storefront: no workspace header → empty cart shape.</summary>
    public async Task<CartDetail> GetCurrentOrEmptyAsync(int? workspaceId, CancellationToken ct)
    {
        if (!workspaceId.HasValue)
            return new CartDetail(0, 0, null, StatusOpen, Array.Empty<CartLineDto>(), "0.00");
        var cart = await db.Carts.OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId.Value, ct);
        if (cart == null)
            return new CartDetail(0, workspaceId.Value, null, StatusOpen, Array.Empty<CartLineDto>(), "0.00");
        return await BuildDetailAsync(cart, ct);
    }

    public async Task<CartDetail> CreateAsync(int workspaceId, CancellationToken ct)
    {
        var c = new Cart { WorkspaceId = workspaceId, SessionKey = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Carts.Add(c);
        await db.SaveChangesAsync(ct);
        return new CartDetail(c.Id, c.WorkspaceId, c.CustomerId, StatusOpen, Array.Empty<CartLineDto>(), "0.00");
    }

    public async Task<CartMutationResult> AddItemAsync(
        int workspaceId,
        int productId,
        int? variantId,
        int quantity,
        CartAddConstraints constraints,
        CancellationToken ct)
    {
        if (quantity < constraints.MinQuantity)
            return CartMutationResult.Fail("bad_request", $"Quantity must be at least {constraints.MinQuantity}.");
        if (constraints.MaxQuantity is { } max && quantity > max)
            return CartMutationResult.Fail("bad_request", $"Quantity cannot exceed {max}.");

        var cart = await db.Carts.OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId, ct);
        if (cart == null)
        {
            cart = new Cart { WorkspaceId = workspaceId, SessionKey = "", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            db.Carts.Add(cart);
            await db.SaveChangesAsync(ct);
        }

        var prod = await db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId && p.WorkspaceId == workspaceId, ct);
        if (prod == null)
            return CartMutationResult.Fail("not_found", "Product not found.");

        decimal unitPrice = prod.Price;
        if (variantId.HasValue)
        {
            var v = await db.Variants.AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == variantId.Value && v.ProductId == productId, ct);
            if (v != null)
                unitPrice = v.Price ?? prod.Price;
        }

        var existing = await db.CartItems.FirstOrDefaultAsync(
            i => i.CartId == cart.Id && i.ProductId == productId && i.VariantId == variantId, ct);
        if (existing != null)
        {
            existing.Quantity += quantity;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.CartItems.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                VariantId = variantId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
        var detail = await BuildDetailAsync(cart, ct);
        return CartMutationResult.Ok(detail);
    }

    public async Task<CartMutationResult> RemoveLineAsync(int lineItemId, CancellationToken ct)
    {
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == lineItemId, ct);
        if (item == null)
            return CartMutationResult.Fail("not_found", "Item not found.");
        var cartId = item.CartId;
        db.CartItems.Remove(item);
        await db.SaveChangesAsync(ct);
        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cartId, ct);
        var detail = await BuildDetailForCartIdAsync(cartId, cart, ct);
        return CartMutationResult.Ok(detail);
    }

    public async Task<CartMutationResult> UpdateLineQuantityAsync(int lineItemId, int quantity, CancellationToken ct)
    {
        var item = await db.CartItems.FirstOrDefaultAsync(i => i.Id == lineItemId, ct);
        if (item == null)
            return CartMutationResult.Fail("not_found", "Item not found.");
        if (quantity <= 0)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync(ct);
        }
        else
        {
            item.Quantity = quantity;
            item.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }

        var cart = await db.Carts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == item.CartId, ct);
        var detail = await BuildDetailForCartIdAsync(item.CartId, cart, ct);
        return CartMutationResult.Ok(detail);
    }

    public async Task<CartDetail> ClearCurrentCartAsync(int workspaceId, CancellationToken ct)
    {
        var cart = await db.Carts.OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId, ct);
        if (cart == null)
            return new CartDetail(0, workspaceId, null, StatusOpen, Array.Empty<CartLineDto>(), "0.00");
        var toRemove = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        db.CartItems.RemoveRange(toRemove);
        await db.SaveChangesAsync(ct);
        return new CartDetail(cart.Id, cart.WorkspaceId, cart.CustomerId, StatusOpen, Array.Empty<CartLineDto>(), "0.00");
    }

    private async Task<CartDetail> BuildDetailAsync(Cart cart, CancellationToken ct)
    {
        var lines = await LoadLinesAsync(cart.Id, ct);
        var total = await db.CartItems.Where(i => i.CartId == cart.Id).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        return new CartDetail(cart.Id, cart.WorkspaceId, cart.CustomerId, StatusOpen, lines, total.ToString("F2"));
    }

    private async Task<CartDetail> BuildDetailForCartIdAsync(int cartId, Cart? cartMeta, CancellationToken ct)
    {
        var lines = await LoadLinesAsync(cartId, ct);
        var total = await db.CartItems.Where(i => i.CartId == cartId).SumAsync(i => i.Quantity * i.UnitPrice, ct);
        return new CartDetail(
            cartId,
            cartMeta?.WorkspaceId ?? 0,
            cartMeta?.CustomerId,
            StatusOpen,
            lines,
            total.ToString("F2"));
    }

    private async Task<IReadOnlyList<CartLineDto>> LoadLinesAsync(int cartId, CancellationToken ct)
    {
        // Newest lines first so storefront clients that append without clearing see the last add_item as items[0].
        var rows = await db.CartItems.AsNoTracking().Where(i => i.CartId == cartId).OrderByDescending(i => i.Id)
            .Select(i => new { i.Id, i.ProductId, i.VariantId, i.Quantity, i.UnitPrice }).ToListAsync(ct);
        var variantIds = rows.Where(r => r.VariantId.HasValue).Select(r => r.VariantId!.Value).Distinct().ToList();
        Dictionary<int, string> optionsByVariantId = new();
        if (variantIds.Count > 0)
        {
            optionsByVariantId = await db.Variants.AsNoTracking()
                .Where(v => variantIds.Contains(v.Id))
                .ToDictionaryAsync(v => v.Id, v => v.Options, ct);
        }

        return rows.Select(i => new CartLineDto(
                i.Id,
                i.ProductId,
                i.VariantId,
                i.Quantity,
                i.UnitPrice.ToString("F2"),
                (i.Quantity * i.UnitPrice).ToString("F2"),
                ParseVariantOptions(
                    i.VariantId is { } vid && optionsByVariantId.TryGetValue(vid, out var opts) ? opts : null)))
            .ToList();
    }

    private static Dictionary<string, string> ParseVariantOptions(string? json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "{}")
            return new Dictionary<string, string>();
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }
}

public sealed record CartListRow(int Id, int Workspace, int? Customer, string Status);

public sealed record CartLineDto(
    int Id,
    int Product,
    int? Variant,
    int Quantity,
    string UnitPrice,
    string TotalPrice,
    Dictionary<string, string> VariantOptions);

public sealed record CartDetail(int Id, int Workspace, int? Customer, string Status, IReadOnlyList<CartLineDto> Items, string Total);

public sealed record CartAddConstraints(int MinQuantity = 1, int? MaxQuantity = null);

public sealed class CartMutationResult
{
    public bool Success { get; private init; }
    public CartDetail? Detail { get; private init; }
    public string? ErrorCode { get; private init; }
    public string? ErrorMessage { get; private init; }

    public static CartMutationResult Ok(CartDetail detail) => new() { Success = true, Detail = detail };
    public static CartMutationResult Fail(string code, string message) => new() { Success = false, ErrorCode = code, ErrorMessage = message };
}
