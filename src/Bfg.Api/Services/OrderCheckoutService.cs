using Bfg.Core;
using Bfg.Core.Shop;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Services;

/// <summary>
/// Creates an order from the workspace cart (explicit id from header or latest by UpdatedAt).
/// </summary>
public sealed class OrderCheckoutService(BfgDbContext db)
{
    public async Task<OrderCheckoutResult> CreateFromCurrentCartAsync(
        int workspaceId,
        int? preferredCartId,
        int? authenticatedUserId,
        int storeId,
        int shippingAddressId,
        int? billingAddressId,
        string? customerNote,
        string? couponCode,
        string? giftCardCode,
        bool validateStoreInWorkspace,
        string? storefrontSessionKey,
        CancellationToken ct)
    {
        if (validateStoreInWorkspace)
        {
            var storeOk = await db.Stores.AsNoTracking()
                .AnyAsync(s => s.Id == storeId && s.WorkspaceId == workspaceId, ct);
            if (!storeOk)
                return OrderCheckoutResult.Fail("store_not_found", "Store not found.");
        }

        Cart? cart = preferredCartId is int pcid && pcid > 0
            ? await db.Carts.FirstOrDefaultAsync(c => c.Id == pcid && c.WorkspaceId == workspaceId, ct)
            : null;
        if (cart == null && !string.IsNullOrWhiteSpace(storefrontSessionKey))
        {
            cart = await db.Carts.OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefaultAsync(
                    c => c.WorkspaceId == workspaceId && c.SessionKey == storefrontSessionKey,
                    ct);
        }

        if (cart == null)
        {
            cart = await db.Carts.OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId, ct);
        }
        if (cart == null)
            return OrderCheckoutResult.Fail("empty_cart", "Cart is empty.");

        var cartItems = await db.CartItems.Where(i => i.CartId == cart.Id).ToListAsync(ct);
        if (cartItems.Count == 0)
            return OrderCheckoutResult.Fail("empty_cart", "Cart is empty.");

        if (!await db.Addresses.AsNoTracking()
                .AnyAsync(a => a.Id == shippingAddressId && a.WorkspaceId == workspaceId, ct))
            return OrderCheckoutResult.Fail("bad_request", "Invalid shipping address.");
        var billId = billingAddressId ?? shippingAddressId;
        if (!await db.Addresses.AsNoTracking()
                .AnyAsync(a => a.Id == billId && a.WorkspaceId == workspaceId, ct))
            return OrderCheckoutResult.Fail("bad_request", "Invalid billing address.");

        var customerId = await ResolveCustomerIdAsync(db, workspaceId, cart, authenticatedUserId, ct);
        if (customerId == 0)
            return OrderCheckoutResult.Fail("customer_required", "Customer required.");

        var productIds = cartItems.Select(i => i.ProductId).Distinct().ToList();
        var catRows = await db.ProductCategoryProducts.AsNoTracking()
            .Where(pcp => productIds.Contains(pcp.ProductId))
            .Select(pcp => new { pcp.ProductId, pcp.ProductCategoryId })
            .ToListAsync(ct);
        var catByProduct = catRows
            .GroupBy(x => x.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProductCategoryId).ToList());

        var calcItems = cartItems.Select(i => new CheckoutTotalsCalculator.CartItemForCalc
        {
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            ProductId = i.ProductId,
            CategoryIds = catByProduct.GetValueOrDefault(i.ProductId) ?? new List<int>()
        }).ToList();

        CheckoutTotalsCalculator.CalcResult calc;
        try
        {
            calc = await CheckoutTotalsCalculator.CalculateAsync(
                db,
                workspaceId,
                calcItems,
                new CheckoutTotalsCalculator.CalcInput { CouponCode = couponCode, GiftCardCode = giftCardCode },
                ct);
        }
        catch (CheckoutCalcException ex)
        {
            return OrderCheckoutResult.Fail("bad_request", ex.Message);
        }

        var orderNum = await OrderNumberService.GenerateAsync(ord => db.Orders.AnyAsync(o => o.OrderNumber == ord, ct));
        var order = new Order
        {
            WorkspaceId = workspaceId,
            CustomerId = customerId,
            StoreId = storeId,
            OrderNumber = orderNum,
            Status = "pending",
            PaymentStatus = "pending",
            Subtotal = calc.Subtotal,
            ShippingCost = calc.ShippingCost,
            Tax = calc.Tax,
            Discount = calc.Discount,
            TotalAmount = calc.TotalAmount,
            ShippingAddressId = shippingAddressId,
            BillingAddressId = billId,
            CustomerNote = customerNote ?? "",
            AdminNote = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);

        foreach (var it in cartItems)
        {
            var prod = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == it.ProductId, ct);
            var variant = it.VariantId.HasValue
                ? await db.Variants.AsNoTracking().FirstOrDefaultAsync(v => v.Id == it.VariantId.Value, ct)
                : null;
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
                TotalPrice = it.Quantity * it.UnitPrice
            });
        }

        db.CartItems.RemoveRange(cartItems);
        await db.SaveChangesAsync(ct);

        // Decrement inventory for products/variants that track stock
        foreach (var it in cartItems)
        {
            if (it.VariantId.HasValue)
            {
                var variant = await db.Variants.FirstOrDefaultAsync(v => v.Id == it.VariantId.Value, ct);
                if (variant != null)
                {
                    variant.StockQuantity = Math.Max(0, variant.StockQuantity - it.Quantity);
                }
            }
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == it.ProductId, ct);
            if (product != null && product.TrackInventory)
            {
                product.StockQuantity = Math.Max(0, product.StockQuantity - it.Quantity);
            }
        }
        await db.SaveChangesAsync(ct);

        if (calc.CouponIdToIncrement is { } couponRowId)
        {
            var v = await db.Vouchers.FirstOrDefaultAsync(x => x.Id == couponRowId, ct);
            if (v != null)
            {
                v.TimesUsed += 1;
                v.UpdatedAt = DateTime.UtcNow;
            }
        }

        if (calc.GiftCardUpdate is { } gcu)
        {
            var gc = await db.GiftCards.FirstOrDefaultAsync(x => x.Id == gcu.Id, ct);
            if (gc != null)
            {
                gc.Balance = gcu.NewBalance;
                gc.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);

        var orderItemRowsRaw = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == order.Id).OrderBy(i => i.Id)
            .Select(i => new { i.Id, i.ProductId, i.VariantId, i.Quantity, i.UnitPrice, i.TotalPrice }).ToListAsync(ct);
        var orderItemRows = orderItemRowsRaw
            .Select(i => new OrderCheckoutLineDto(i.Id, i.ProductId, i.VariantId, i.Quantity, i.UnitPrice.ToString("F2"), i.TotalPrice.ToString("F2")))
            .ToList();

        return OrderCheckoutResult.Ok(new OrderCheckoutPayload(order, orderItemRows));
    }

    private static async Task<int> ResolveCustomerIdAsync(
        BfgDbContext db,
        int workspaceId,
        Cart cart,
        int? authenticatedUserId,
        CancellationToken ct)
    {
        if (authenticatedUserId is int uid && uid > 0)
        {
            var linked = await db.Customers.AsNoTracking()
                .Where(c => c.WorkspaceId == workspaceId && c.UserId == uid)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(ct);
            if (linked != 0)
                return linked;
        }

        if (cart.CustomerId is int cid && cid > 0)
            return cid;

        return await db.Customers.AsNoTracking()
            .Where(c => c.WorkspaceId == workspaceId)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(ct);
    }
}
