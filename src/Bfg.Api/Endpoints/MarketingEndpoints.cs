using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Promo;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class MarketingEndpoints
{
    public static void MapMarketingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/marketing").WithTags("Marketing").RequireAuthorization();

        group.MapGet("/campaigns", ListCampaigns);
        group.MapPost("/campaigns/", CreateCampaign);
        group.MapGet("/campaigns/{id:int}", GetCampaign);
        group.MapPatch("/campaigns/{id:int}", PatchCampaign);

        group.MapGet("/discount-rules", ListDiscountRules);
        group.MapPost("/discount-rules/", CreateDiscountRule);

        group.MapGet("/coupons", ListCoupons);
        group.MapPost("/coupons/", CreateCoupon);
        group.MapPatch("/coupons/{id:int}", PatchCoupon);

        group.MapGet("/gift-cards", ListGiftCards);
        group.MapPost("/gift-cards/", CreateGiftCard);
        group.MapPost("/gift-cards/{id:int}/redeem/", RedeemGiftCard);
    }

    private static async Task<IResult> ListCampaigns(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Campaigns.AsNoTracking().Where(c => !wid.HasValue || c.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var (page, pageSize) = Pagination.FromRequest(req);
        var list = await query.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new { id = c.Id, name = c.Name, campaign_type = c.CampaignType, description = c.Description, start_date = c.StartDate, end_date = c.EndDate, budget = c.Budget, is_active = c.IsActive }).ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateCampaign(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CampaignCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var c = new Campaign
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            CampaignType = body.campaign_type ?? "email",
            Description = body.description,
            StartDate = body.start_date,
            EndDate = body.end_date,
            Budget = decimal.TryParse(body.budget, out var b) ? b : null,
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Campaigns.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/marketing/campaigns/", new { id = c.Id, name = c.Name, campaign_type = c.CampaignType });
    }

    private static async Task<IResult> GetCampaign(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Campaigns.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, name = c.Name, campaign_type = c.CampaignType, budget = c.Budget });
    }

    private static async Task<IResult> PatchCampaign(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Campaigns.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CampaignPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) c.Name = body.name;
            if (body.budget != null && decimal.TryParse(body.budget, out var b)) c.Budget = b;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, name = c.Name, budget = c.Budget?.ToString("F2") });
    }

    private static async Task<IResult> ListDiscountRules(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.DiscountRules.AsNoTracking().Where(r => !wid.HasValue || r.WorkspaceId == wid.Value)
            .Select(r => new { id = r.Id, name = r.Name, discount_type = r.DiscountType, discount_value = r.DiscountValue.ToString("F2"), apply_to = r.ApplyTo, is_active = r.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateDiscountRule(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<DiscountRuleCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var r = new DiscountRule
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            DiscountType = body.discount_type ?? "percentage",
            DiscountValue = decimal.TryParse(body.discount_value, out var v) ? v : 0,
            ApplyTo = body.apply_to ?? "order",
            MaximumDiscount = body.maximum_discount != null && decimal.TryParse(body.maximum_discount, out var md) ? md : null,
            MinimumPurchase = body.minimum_purchase != null && decimal.TryParse(body.minimum_purchase, out var mp) ? mp : null,
            DisplayLabel = body.display_label,
            IsActive = body.is_active ?? true,
            ValidFrom = body.valid_from,
            ValidUntil = body.valid_until,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.DiscountRules.Add(r);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/marketing/discount-rules/", new { id = r.Id, name = r.Name, discount_type = r.DiscountType, discount_value = r.DiscountValue.ToString("F2") });
    }

    private static async Task<IResult> ListCoupons(BfgDbContext db, HttpContext ctx, bool? is_active, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Vouchers.AsNoTracking().Where(v => (!wid.HasValue || v.WorkspaceId == wid.Value) && v.DiscountRuleId != null);
        if (is_active.HasValue) query = query.Where(v => v.IsActive == is_active.Value);
        var list = await query.Select(v => new { id = v.Id, code = v.Code, discount_rule_id = v.DiscountRuleId, valid_from = v.ValidFrom, valid_until = v.ValidUntil, is_active = v.IsActive, usage_limit = v.UsageLimit }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateCoupon(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CouponCreateBody>(ct);
        if (body == null || body.discount_rule_id <= 0) return Results.BadRequest();
        var v = new Voucher
        {
            WorkspaceId = wid.Value,
            DiscountRuleId = body.discount_rule_id,
            CampaignId = body.campaign_id,
            Code = body.code ?? "",
            Description = body.description,
            DiscountValue = 0,
            ValidFrom = body.valid_from,
            ValidUntil = body.valid_until,
            UsageLimit = body.usage_limit,
            UsageLimitPerCustomer = body.usage_limit_per_customer,
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Vouchers.Add(v);
        await db.SaveChangesAsync(ct);
        var dr = await db.DiscountRules.AsNoTracking().FirstOrDefaultAsync(r => r.Id == body.discount_rule_id, ct);
        return Results.Created("/api/v1/marketing/coupons/", new { id = v.Id, code = v.Code, discount_rule = new { id = v.DiscountRuleId } });
    }

    private static async Task<IResult> PatchCoupon(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var v = await db.Vouchers.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (v == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CouponPatchBody>(ct);
        if (body?.usage_limit != null) { v.UsageLimit = body.usage_limit; v.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
        return Results.Ok(new { id = v.Id, usage_limit = v.UsageLimit });
    }

    private static async Task<IResult> ListGiftCards(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.GiftCards.AsNoTracking().Where(g => !wid.HasValue || g.WorkspaceId == wid.Value)
            .Select(g => new { id = g.Id, code = g.Code, initial_value = g.InitialValue.ToString("F2"), balance = g.Balance.ToString("F2"), is_active = g.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateGiftCard(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<GiftCardCreateBody>(ct);
        if (body == null || body.currency <= 0) return Results.BadRequest();
        var initial = decimal.TryParse(body.initial_value, out var iv) ? iv : 0;
        var balance = decimal.TryParse(body.balance, out var bv) ? bv : initial;
        var g = new GiftCard
        {
            WorkspaceId = wid.Value,
            CustomerId = body.customer,
            CurrencyId = body.currency,
            InitialValue = initial,
            Balance = balance,
            Code = "GC-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.GiftCards.Add(g);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/marketing/gift-cards/", new { id = g.Id, code = g.Code, initial_value = g.InitialValue.ToString("F2"), balance = g.Balance.ToString("F2") });
    }

    private static async Task<IResult> RedeemGiftCard(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var g = await db.GiftCards.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (g == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<RedeemBody>(ct);
        var amount = body != null && decimal.TryParse(body.amount, out var a) ? a : 0;
        if (amount <= 0 || amount > g.Balance) return Results.BadRequest();
        g.Balance -= amount;
        g.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { success = true, redeemed_amount = amount.ToString("F2"), remaining_balance = g.Balance.ToString("F2"), gift_card = new { balance = g.Balance.ToString("F2") } });
    }

    private sealed record CampaignCreateBody(string? name, string? campaign_type, string? description, DateTime? start_date, DateTime? end_date, string? budget, bool? is_active);
    private sealed record CampaignPatchBody(string? name, string? budget);
    private sealed record DiscountRuleCreateBody(string? name, string? discount_type, string? discount_value, string? apply_to, string? maximum_discount, string? minimum_purchase, string? display_label, bool? is_active, DateTime? valid_from, DateTime? valid_until);
    private sealed record CouponCreateBody(int discount_rule_id, int? campaign_id, string? code, string? description, DateTime? valid_from, DateTime? valid_until, int? usage_limit, int? usage_limit_per_customer, bool? is_active);
    private sealed record CouponPatchBody(int? usage_limit);
    private sealed record GiftCardCreateBody(string? initial_value, string? balance, int currency, int? customer, bool? is_active);
    private sealed record RedeemBody(string? amount);
}
