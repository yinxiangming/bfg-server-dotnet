using System.Text.Json;
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
        var root = app.MapGroup("/api/v1").WithTags("Marketing").RequireAuthorization();

        group.MapGet("/campaigns", ListCampaigns);
        group.MapPost("/campaigns/", CreateCampaign);
        group.MapGet("/campaigns/{id:int}", GetCampaign);
        group.MapPatch("/campaigns/{id:int}", PatchCampaign);

        root.MapGet("/campaigns", ListCampaigns);
        root.MapPost("/campaigns/", CreateCampaign);
        root.MapGet("/campaigns/{id:int}", GetCampaign);
        root.MapPatch("/campaigns/{id:int}", PatchCampaign);

        group.MapGet("/discount-rules", ListDiscountRules);
        group.MapPost("/discount-rules/", CreateDiscountRule);

        root.MapGet("/discount-rules", ListDiscountRules);
        root.MapPost("/discount-rules/", CreateDiscountRule);

        group.MapGet("/coupons", ListCoupons);
        group.MapPost("/coupons/", CreateCoupon);
        group.MapPatch("/coupons/{id:int}", PatchCoupon);

        root.MapGet("/coupons", ListCoupons);
        root.MapPost("/coupons/", CreateCoupon);
        root.MapPatch("/coupons/{id:int}", PatchCoupon);

        group.MapGet("/gift-cards", ListGiftCards);
        group.MapPost("/gift-cards/", CreateGiftCard);
        group.MapPost("/gift-cards/{id:int}/redeem/", RedeemGiftCard);

        root.MapGet("/gift-cards", ListGiftCards);
        root.MapPost("/gift-cards/", CreateGiftCard);
        root.MapPost("/gift-cards/{id:int}/redeem/", RedeemGiftCard);

        group.MapPost("/campaign-displays/", CreateCampaignDisplay);
        root.MapPost("/campaign-displays/", CreateCampaignDisplay);

        var promo = app.MapGroup("/api/v1/promo").WithTags("Promo").RequireAuthorization();
        promo.MapGet("/vouchers", ListVouchers);
        promo.MapPost("/vouchers/", CreateVoucher);
        promo.MapGet("/campaigns", ListCampaigns);
        promo.MapPost("/campaigns/", CreateCampaign);
    }

    private static string Trunc(string? s, int maxLen) =>
        string.IsNullOrEmpty(s) ? "" : (s.Length <= maxLen ? s : s[..maxLen]);

    private static async Task<IResult> ListVouchers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.Vouchers.AsNoTracking()
            .Where(v => !wid.HasValue || v.WorkspaceId == wid.Value)
            .Join(db.DiscountRules.AsNoTracking(), v => v.DiscountRuleId, r => r.Id, (v, r) => new { v, r })
            .Where(x => !wid.HasValue || x.r.WorkspaceId == wid.Value)
            .Select(x => new
            {
                id = x.v.Id,
                code = x.v.Code,
                discount_type = x.r.DiscountType,
                discount_value = x.r.DiscountValue.ToString("F2"),
                is_active = x.v.IsActive
            })
            .ToListAsync(ct);
        return Results.Ok(new { count = list.Count, next = (string?)null, previous = (string?)null, results = list });
    }

    private static async Task<IResult> CreateVoucher(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<VoucherCreateBody>(ct);
        if (body == null || body.discount_rule_id <= 0) return Results.BadRequest();
        var rule = await db.DiscountRules.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == body.discount_rule_id && r.WorkspaceId == wid.Value, ct);
        if (rule == null) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var v = new Voucher
        {
            WorkspaceId = wid.Value,
            DiscountRuleId = body.discount_rule_id,
            Code = (body.code ?? "").Trim(),
            Description = "",
            TimesUsed = 0,
            ValidFrom = now,
            ValidUntil = null,
            UsageLimit = body.usage_limit,
            IsActive = body.is_active ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Vouchers.Add(v);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/promo/vouchers/", new
        {
            id = v.Id,
            code = v.Code,
            discount_type = rule.DiscountType,
            discount_value = rule.DiscountValue.ToString("F2"),
            is_active = v.IsActive
        });
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
        var now = DateTime.UtcNow;
        var c = new Campaign
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            CampaignType = Trunc(body.campaign_type ?? "email", 20),
            Description = body.description ?? "",
            StartDate = body.start_date ?? now,
            EndDate = body.end_date,
            Budget = decimal.TryParse(body.budget, out var b) ? b : null,
            UtmSource = Trunc(body.utm_source, 100),
            UtmMedium = Trunc(body.utm_medium, 100),
            UtmCampaign = Trunc(body.utm_campaign, 100),
            IsActive = body.is_active ?? true,
            RequiresParticipation = body.requires_participation ?? false,
            MinParticipants = body.min_participants,
            MaxParticipants = body.max_participants,
            PromoDisplayType = Trunc(body.promo_display_type, 50),
            Config = "{}",
            CreatedAt = now,
            UpdatedAt = now
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
        var discountVal = decimal.TryParse(body.discount_value, out var v) ? v : 0;
        if (discountVal < 0)
            return Results.BadRequest(new { discount_value = new[] { "Negative discount_value is not allowed." } });

        var entitledJson = "[]";
        if (body.product_ids is { Count: > 0 })
            entitledJson = JsonSerializer.Serialize(body.product_ids);
        var prerequisiteJson = "[]";
        if (body.category_ids is { Count: > 0 })
            prerequisiteJson = JsonSerializer.Serialize(body.category_ids);

        var displayLabel = body.display_label ?? body.name ?? "";
        var cfgParts = new Dictionary<string, object>();
        if (body.product_ids is { Count: > 0 }) cfgParts["product_ids"] = body.product_ids;
        if (body.category_ids is { Count: > 0 }) cfgParts["category_ids"] = body.category_ids;
        var configJson = cfgParts.Count > 0 ? JsonSerializer.Serialize(cfgParts) : "{}";
        var now = DateTime.UtcNow;
        var r = new DiscountRule
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            DiscountType = Trunc(body.discount_type ?? "percentage", 20),
            DiscountValue = discountVal,
            ApplyTo = Trunc(body.apply_to ?? "order", 20),
            MaximumDiscount = body.maximum_discount != null && decimal.TryParse(body.maximum_discount, out var md) ? md : null,
            MinimumPurchase = body.minimum_purchase != null && decimal.TryParse(body.minimum_purchase, out var mp) ? mp : null,
            Config = configJson,
            PrerequisiteProductIds = prerequisiteJson,
            EntitledProductIds = entitledJson,
            AllocationMethod = Trunc("across", 20),
            DisplayLabel = displayLabel,
            IsGroupBuy = false,
            IsActive = body.is_active ?? true,
            ValidFrom = body.valid_from,
            ValidUntil = body.valid_until,
            CreatedAt = now
        };
        db.DiscountRules.Add(r);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/marketing/discount-rules/", new { id = r.Id, name = r.Name, discount_type = r.DiscountType, discount_value = r.DiscountValue.ToString("F2") });
    }

    private static async Task<IResult> ListCoupons(BfgDbContext db, HttpContext ctx, bool? is_active, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Vouchers.AsNoTracking().Where(v => !wid.HasValue || v.WorkspaceId == wid.Value);
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
        var now = DateTime.UtcNow;
        var validFrom = CoerceCouponValidFrom(body.valid_from, now);
        var validUntil = body.valid_until.HasValue ? NormalizeToUtc(body.valid_until.Value) : (DateTime?)null;
        var v = new Voucher
        {
            WorkspaceId = wid.Value,
            DiscountRuleId = body.discount_rule_id,
            CampaignId = body.campaign_id,
            Code = (body.code ?? "").Trim(),
            Description = body.description ?? "",
            TimesUsed = 0,
            ValidFrom = validFrom,
            ValidUntil = validUntil,
            UsageLimit = body.usage_limit,
            UsageLimitPerCustomer = body.usage_limit_per_customer,
            IsActive = body.is_active ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Vouchers.Add(v);
        await db.SaveChangesAsync(ct);
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
        if (initial < 0) return Results.BadRequest(new { initial_value = new[] { "Must be non-negative." } });
        if (balance < 0) return Results.BadRequest(new { balance = new[] { "Must be non-negative." } });
        if (balance > initial) return Results.BadRequest(new { balance = new[] { "Balance cannot exceed initial_value." } });
        var now = DateTime.UtcNow;
        var g = new GiftCard
        {
            WorkspaceId = wid.Value,
            CustomerId = body.customer,
            CurrencyId = body.currency,
            InitialValue = initial,
            Balance = balance,
            Code = "GC-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            Note = "",
            IsActive = body.is_active ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.GiftCards.Add(g);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/marketing/gift-cards/", new { id = g.Id, code = g.Code, initial_value = g.InitialValue.ToString("F2"), balance = g.Balance.ToString("F2") });
    }

    private static async Task<IResult> CreateCampaignDisplay(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CampaignDisplayCreateBody>(ct);
        if (body == null || body.campaign is null or <= 0) return Results.BadRequest();
        var camp = await db.Campaigns.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == body.campaign && c.WorkspaceId == wid.Value, ct);
        if (camp == null) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var title = string.IsNullOrEmpty(body.title) ? camp.Name : body.title!;
        var d = new CampaignDisplay
        {
            DisplayType = Trunc(body.display_type ?? "slide", 30),
            SortOrder = body.order ?? 0,
            Title = title,
            Subtitle = body.subtitle ?? "",
            Image = null,
            LinkUrl = body.link_url ?? "",
            LinkTarget = "_self",
            Rules = "{}",
            IsActive = body.is_active ?? true,
            CampaignId = body.campaign,
            PostId = null,
            WorkspaceId = wid.Value,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.CampaignDisplays.Add(d);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/marketing/campaign-displays/", new { id = d.Id, campaign = d.CampaignId, display_type = d.DisplayType });
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

    private sealed record CampaignDisplayCreateBody(int? campaign, string? display_type, int? order, string? link_url, bool? is_active, string? title, string? subtitle);

    private sealed record CampaignCreateBody(
        string? name,
        string? campaign_type,
        string? description,
        DateTime? start_date,
        DateTime? end_date,
        string? budget,
        bool? is_active,
        string? utm_source,
        string? utm_medium,
        string? utm_campaign,
        bool? requires_participation,
        int? min_participants,
        int? max_participants,
        string? promo_display_type);

    private sealed record CampaignPatchBody(string? name, string? budget);

    private sealed class DiscountRuleCreateBody
    {
        public string? name { get; set; }
        public string? discount_type { get; set; }
        public string? discount_value { get; set; }
        public string? apply_to { get; set; }
        public string? maximum_discount { get; set; }
        public string? minimum_purchase { get; set; }
        public string? display_label { get; set; }
        public bool? is_active { get; set; }
        public DateTime? valid_from { get; set; }
        public DateTime? valid_until { get; set; }
        public List<int>? product_ids { get; set; }
        public List<int>? category_ids { get; set; }
    }

    private sealed record CouponCreateBody(int discount_rule_id, int? campaign_id, string? code, string? description, DateTime? valid_from, DateTime? valid_until, int? usage_limit, int? usage_limit_per_customer, bool? is_active);

    private static DateTime NormalizeToUtc(DateTime dt) => dt.Kind switch
    {
        DateTimeKind.Utc => dt,
        DateTimeKind.Local => dt.ToUniversalTime(),
        _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
    };

    /// <summary>
    /// Clamp valid_from so checkout never sees "not yet valid" when client clock is ahead of the API host.
    /// </summary>
    private static DateTime CoerceCouponValidFrom(DateTime? bodyFrom, DateTime utcNow)
    {
        var vf = bodyFrom.HasValue ? NormalizeToUtc(bodyFrom.Value) : utcNow;
        if (vf > utcNow)
            vf = utcNow;
        return vf;
    }
    private sealed record CouponPatchBody(int? usage_limit);
    private sealed record GiftCardCreateBody(string? initial_value, string? balance, int currency, int? customer, bool? is_active);
    private sealed record RedeemBody(string? amount);
    private sealed record VoucherCreateBody(string? code, int discount_rule_id, bool? is_active, int? usage_limit);
}
