using System.Text.Json;
using Bfg.Core;
using Bfg.Core.Promo;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Services;

/// <summary>
/// Port of bfg-server-nodejs checkoutCalculation.ts for e2e parity.
/// </summary>
public static class CheckoutTotalsCalculator
{
    public sealed class CartItemForCalc
    {
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public int ProductId { get; init; }
        public IReadOnlyList<int> CategoryIds { get; init; } = Array.Empty<int>();
    }

    public sealed class CalcInput
    {
        public string? CouponCode { get; init; }
        public string? GiftCardCode { get; init; }
    }

    public sealed class CalcResult
    {
        public decimal Subtotal { get; init; }
        public decimal ShippingCost { get; init; }
        public decimal Tax { get; init; }
        public decimal Discount { get; init; }
        public decimal TotalAmount { get; init; }
        public int? CouponIdToIncrement { get; init; }
        public (int Id, decimal NewBalance)? GiftCardUpdate { get; init; }
    }

    public static decimal DefaultTaxAmount(decimal subtotal)
    {
        if (subtotal >= 400) return Round2(subtotal * 0.05m);
        return 5m;
    }

    public static async Task<CalcResult> CalculateAsync(
        BfgDbContext db,
        int workspaceId,
        IReadOnlyList<CartItemForCalc> items,
        CalcInput input,
        CancellationToken ct)
    {
        var subtotal = Round2(items.Sum(i => i.Quantity * i.UnitPrice));
        var tax = DefaultTaxAmount(subtotal);
        var shippingCost = 10m;

        var rules = await db.DiscountRules.AsNoTracking()
            .Where(r => r.WorkspaceId == workspaceId && r.IsActive)
            .OrderBy(r => r.Id)
            .ToListAsync(ct);

        var couponTrim = input.CouponCode?.Trim();
        if (string.IsNullOrEmpty(couponTrim))
        {
            foreach (var r in rules)
            {
                if (r.DiscountType != "free_shipping" || r.ApplyTo != "order") continue;
                var min = r.MinimumPurchase ?? 0;
                if (subtotal + 0.0000001m >= min)
                {
                    shippingCost = 0;
                    break;
                }
            }
        }

        var monetaryFromRules = 0m;
        int? couponIdToIncrement = null;

        if (!string.IsNullOrEmpty(couponTrim))
        {
            var now = DateTime.UtcNow;
            // Load active coupons for workspace; filter validity in-memory so MySQL naive datetimes
            // (Django UTC storage) compare consistently with UtcNow.
            var rows = await db.Vouchers.AsNoTracking()
                .Where(x => x.WorkspaceId == workspaceId && x.IsActive)
                .OrderByDescending(x => x.Id)
                .ToListAsync(ct);
            var v = rows.FirstOrDefault(x =>
                string.Equals((x.Code ?? "").Trim(), couponTrim, StringComparison.OrdinalIgnoreCase)
                && IsVoucherWithinValidityWindow(x, now));
            if (v == null)
                throw new CheckoutCalcException("Invalid or expired coupon.");
            var couponRule = await db.DiscountRules.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == v.DiscountRuleId, ct);
            if (couponRule == null)
                throw new CheckoutCalcException("Invalid or expired coupon.");
            if (v.UsageLimit != null && v.TimesUsed >= v.UsageLimit)
                throw new CheckoutCalcException("Coupon usage limit exceeded.");
            if (!couponRule.IsActive)
                throw new CheckoutCalcException("Invalid or expired coupon.");

            var cfg = ParseRuleConfig(couponRule.Config);
            var eligible = EligibleSubtotalForRule(couponRule.ApplyTo, cfg, items);
            var minPur = couponRule.MinimumPurchase ?? 0;
            if (eligible + 0.0000001m < minPur)
                throw new CheckoutCalcException("Order does not meet minimum purchase for this coupon.");

            if (couponRule.DiscountType == "free_shipping")
                shippingCost = 0;
            else
                monetaryFromRules = RuleMonetaryDiscount(couponRule.DiscountType, couponRule.DiscountValue, couponRule.MaximumDiscount, eligible);

            couponIdToIncrement = v.Id;
        }
        else
        {
            var best = 0m;
            foreach (var r in rules)
            {
                if (r.DiscountType == "free_shipping") continue;
                var cfg = ParseRuleConfig(r.Config);
                var eligible = EligibleSubtotalForRule(r.ApplyTo, cfg, items);
                var minPur = r.MinimumPurchase ?? 0;
                if (eligible + 0.0000001m < minPur) continue;
                var d = RuleMonetaryDiscount(r.DiscountType, r.DiscountValue, r.MaximumDiscount, eligible);
                if (d > best) best = d;
            }
            monetaryFromRules = best;
        }

        (int Id, decimal NewBalance)? giftCardUpdate = null;
        var giftDiscount = 0m;
        var gcCode = input.GiftCardCode?.Trim();
        if (!string.IsNullOrEmpty(gcCode))
        {
            var gcCandidates = await db.GiftCards.AsNoTracking()
                .Where(g => g.WorkspaceId == workspaceId && g.IsActive)
                .ToListAsync(ct);
            var card = gcCandidates.FirstOrDefault(g => string.Equals(g.Code, gcCode, StringComparison.OrdinalIgnoreCase));
            if (card == null || card.Balance <= 0)
                throw new CheckoutCalcException("Invalid gift card.");

            var prelimTotal = subtotal + shippingCost + tax;
            var room = Round2(Math.Max(0, prelimTotal - monetaryFromRules));
            giftDiscount = Round2(Math.Min(card.Balance, room));
            giftCardUpdate = (card.Id, Round2(card.Balance - giftDiscount));
        }

        var discount = Round2(monetaryFromRules + giftDiscount);
        discount = Round2(Math.Min(discount, subtotal));
        var effective = Round2(Math.Min(discount, subtotal));
        var totalAmount = Round2(subtotal + shippingCost + tax - effective);
        if (totalAmount < 0) totalAmount = 0;

        return new CalcResult
        {
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            Tax = tax,
            Discount = discount,
            TotalAmount = totalAmount,
            CouponIdToIncrement = couponIdToIncrement,
            GiftCardUpdate = giftCardUpdate
        };
    }

    private static decimal Round2(decimal n) => Math.Round(n, 2, MidpointRounding.AwayFromZero);

    private static DateTime NormalizeAsUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };

    /// <summary>
    /// Allow client/API clock skew so coupons created with "now" from the client are not rejected
    /// when the DB round-trip is slightly behind server UtcNow.
    /// </summary>
    private static readonly TimeSpan CouponValidFromClockSkew = TimeSpan.FromMinutes(15);

    private static bool IsVoucherWithinValidityWindow(Voucher v, DateTime utcNow)
    {
        var from = NormalizeAsUtc(v.ValidFrom);
        if (from > utcNow + CouponValidFromClockSkew)
            return false;
        if (v.ValidUntil is { } until)
        {
            var u = NormalizeAsUtc(until);
            if (u < utcNow)
                return false;
        }

        return true;
    }

    private static RuleCfg ParseRuleConfig(string configRaw)
    {
        try
        {
            using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(configRaw) ? "{}" : configRaw);
            var root = doc.RootElement;
            int[]? productIds = null;
            if (root.TryGetProperty("product_ids", out var pEl) && pEl.ValueKind == JsonValueKind.Array)
            {
                var list = new List<int>();
                foreach (var x in pEl.EnumerateArray())
                {
                    if (x.TryGetInt32(out var pi)) list.Add(pi);
                }
                if (list.Count > 0) productIds = list.ToArray();
            }
            int[]? categoryIds = null;
            if (root.TryGetProperty("category_ids", out var cEl) && cEl.ValueKind == JsonValueKind.Array)
            {
                var list = new List<int>();
                foreach (var x in cEl.EnumerateArray())
                {
                    if (x.TryGetInt32(out var ci)) list.Add(ci);
                }
                if (list.Count > 0) categoryIds = list.ToArray();
            }
            return new RuleCfg(productIds, categoryIds);
        }
        catch
        {
            return new RuleCfg(null, null);
        }
    }

    private sealed record RuleCfg(int[]? ProductIds, int[]? CategoryIds);

    private static decimal EligibleSubtotalForRule(string applyTo, RuleCfg cfg, IReadOnlyList<CartItemForCalc> items)
    {
        if (applyTo == "order")
            return items.Sum(i => i.Quantity * i.UnitPrice);
        if (applyTo == "products" && cfg.ProductIds is { Length: > 0 })
        {
            var set = new HashSet<int>(cfg.ProductIds);
            return items.Where(i => set.Contains(i.ProductId)).Sum(i => i.Quantity * i.UnitPrice);
        }
        if (applyTo == "categories" && cfg.CategoryIds is { Length: > 0 })
        {
            var catSet = new HashSet<int>(cfg.CategoryIds);
            return items
                .Where(i => i.CategoryIds.Any(c => catSet.Contains(c)))
                .Sum(i => i.Quantity * i.UnitPrice);
        }
        return 0;
    }

    private static decimal RuleMonetaryDiscount(
        string discountType,
        decimal discountValue,
        decimal? maximumDiscount,
        decimal eligibleSubtotal)
    {
        if (eligibleSubtotal <= 0) return 0;
        if (discountType == "free_shipping") return 0;
        if (discountType == "percentage")
        {
            var d = Round2(eligibleSubtotal * discountValue / 100m);
            if (maximumDiscount is > 0) d = Math.Min(d, maximumDiscount.Value);
            return Round2(d);
        }
        if (discountType == "fixed_amount")
            return Round2(Math.Min(discountValue, eligibleSubtotal));
        return 0;
    }
}

public sealed class CheckoutCalcException(string message) : Exception(message);
