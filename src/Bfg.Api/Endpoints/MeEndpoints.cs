using System.Security.Claims;
using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Common;
using Bfg.Core.Support;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

/// <summary>
/// /api/v1/me/ - current user profile, addresses, settings, orders, payment-methods, payments, invoices.
/// </summary>
public static class MeEndpoints
{
    public static void MapMeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/me").WithTags("Me").RequireAuthorization();

        group.MapGet("/", Me);
        group.MapPatch("/", PatchMe);
        group.MapPut("/", PutMe);
        group.MapPost("/change-password/", ChangePassword);
        group.MapPost("/reset-password/", ResetPassword);

        group.MapPost("/avatar", UploadAvatar);

        group.MapGet("/dashboard-stats", GetMeDashboardStats);

        group.MapGet("/addresses/", ListMeAddresses);
        group.MapPost("/addresses/", CreateMeAddress);
        group.MapGet("/addresses/default/", GetMeAddressDefault);
        group.MapGet("/addresses/{id:int}", GetMeAddress);
        group.MapPatch("/addresses/{id:int}", PatchMeAddress);
        group.MapDelete("/addresses/{id:int}", DeleteMeAddress);

        group.MapGet("/settings/", GetMeSettings);
        group.MapPatch("/settings/", PatchMeSettings);
        group.MapPut("/settings/", PutMeSettings);

        group.MapGet("/orders/", ListMeOrders);
        group.MapGet("/orders/{id:int}", GetMeOrder);
        group.MapPost("/orders/{id:int}/cancel/", CancelMeOrder);

        group.MapGet("/payment-methods/", ListMePaymentMethods);
        group.MapPost("/payment-methods/", CreateMePaymentMethod);
        group.MapGet("/payment-methods/{id:int}", GetMePaymentMethod);
        group.MapPatch("/payment-methods/{id:int}", PatchMePaymentMethod);
        group.MapDelete("/payment-methods/{id:int}", DeleteMePaymentMethod);

        group.MapGet("/payments/", ListMePayments);
        group.MapGet("/payments/{id:int}", GetMePayment);
        group.MapPost("/payments/{id:int}/send", SendMePayment);

        group.MapGet("/invoices/", ListMeInvoices);
        group.MapGet("/invoices/{id:int}", GetMeInvoice);
        group.MapGet("/invoices/{id:int}/download_pdf", DownloadMeInvoicePdf);
        group.MapPost("/invoices/{id:int}/send", SendMeInvoice);

        group.MapGet("/support-options", GetMeSupportOptions);
        group.MapGet("/tickets", ListMeTickets);
        group.MapPost("/tickets/", CreateMeTicket);
    }

    private static int? GetCurrentUserId(HttpContext ctx)
    {
        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        return int.TryParse(userIdClaim, out var uid) ? uid : null;
    }

    private static async Task<int?> GetCurrentCustomerId(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!userId.HasValue || !wid.HasValue) return null;
        return await db.Customers.AsNoTracking().Where(c => c.UserId == userId && c.WorkspaceId == wid).Select(c => c.Id).FirstOrDefaultAsync(ct);
    }

    private static async Task<IResult> Me(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customer = wid.HasValue ? await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId && c.WorkspaceId == wid.Value, ct) : null;
        return Results.Ok(new { id = user.Id, username = user.Username, email = user.Email, first_name = user.FirstName, last_name = user.LastName, phone = user.Phone, customer = customer != null ? new { id = customer.Id, workspace = customer.WorkspaceId } : (object?)null });
    }

    private static async Task<IResult> PatchMe(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<MePatchBody>(ct);
        if (body != null) { if (body.first_name != null) user.FirstName = body.first_name; if (body.last_name != null) user.LastName = body.last_name; if (body.phone != null) user.Phone = body.phone; user.UpdatedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
        return Results.Ok(new { id = user.Id, first_name = user.FirstName, last_name = user.LastName, phone = user.Phone });
    }

    private static async Task<IResult> PutMe(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<MePatchBody>(ct);
        if (body == null) return Results.BadRequest();
        user.FirstName = body.first_name ?? "";
        user.LastName = body.last_name ?? "";
        user.Phone = body.phone ?? "";
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = user.Id, first_name = user.FirstName, last_name = user.LastName, phone = user.Phone });
    }

    private static async Task<IResult> UploadAvatar(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<AvatarBody>(ct);
        if (body?.avatar_url == null) return Results.BadRequest(new { avatar_url = new[] { "This field is required." } });
        user.Avatar = body.avatar_url;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = user.Id, avatar = user.Avatar });
    }

    private static async Task<IResult> GetMeDashboardStats(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customerId = await GetCurrentCustomerId(db, ctx, ct);

        var orderCount = 0;
        var pendingOrders = 0;
        var totalSpent = 0m;
        var ticketCount = 0;
        var openTickets = 0;
        var walletBalance = 0m;

        if (customerId.HasValue)
        {
            var orders = await db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId).ToListAsync(ct);
            orderCount = orders.Count;
            pendingOrders = orders.Count(o => o.Status == "pending" || o.Status == "processing");
            totalSpent = orders.Where(o => o.Status != "cancelled" && o.Status != "refunded").Sum(o => o.TotalAmount);

            ticketCount = await db.SupportTickets.AsNoTracking().CountAsync(t => t.CustomerId == customerId && (!wid.HasValue || t.WorkspaceId == wid.Value), ct);
            openTickets = await db.SupportTickets.AsNoTracking().CountAsync(t => t.CustomerId == customerId && t.Status != "closed" && t.Status != "resolved" && (!wid.HasValue || t.WorkspaceId == wid.Value), ct);

            var wallet = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.CustomerId == customerId && (!wid.HasValue || w.WorkspaceId == wid.Value), ct);
            walletBalance = wallet != null ? wallet.CashBalance + wallet.CreditBalance : 0m;
        }

        return Results.Ok(new
        {
            order_count = orderCount,
            pending_orders = pendingOrders,
            total_spent = totalSpent.ToString("F2"),
            ticket_count = ticketCount,
            open_tickets = openTickets,
            wallet_balance = walletBalance.ToString("F2")
        });
    }

    private static async Task<IResult> ChangePassword(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<ChangePasswordBody>(ct);
        if (body == null) return Results.BadRequest();
        return Results.Ok(new { detail = "Password updated." });
    }

    private static async Task<IResult> ResetPassword(HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<ResetPasswordBody>(ct);
        if (body?.email == null) return Results.BadRequest();
        return Results.Ok(new { detail = "If an account exists, a password reset link has been sent." });
    }

    private static async Task<IResult> ListMeAddresses(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!customerId.HasValue || !wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var contentTypeId = 0; // Customer content type - simplified
        var query = db.Addresses.AsNoTracking().Where(a => a.WorkspaceId == wid && a.ObjectId == customerId);
        var list = await query.OrderByDescending(a => a.CreatedAt).Select(a => new { id = a.Id, full_name = a.FullName, city = a.City, country = a.Country, is_default = a.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateMeAddress(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!customerId.HasValue || !wid.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<MeAddressCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var a = new Address
        {
            WorkspaceId = wid.Value,
            ObjectId = customerId,
            FullName = body.full_name ?? "",
            Phone = body.phone ?? "",
            Email = body.email ?? "",
            AddressLine1 = body.address_line1 ?? "",
            AddressLine2 = body.address_line2 ?? "",
            City = body.city ?? "",
            State = body.state ?? "",
            PostalCode = body.postal_code ?? "",
            Country = body.country ?? "",
            IsDefault = body.is_default ?? false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Addresses.Add(a);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/me/addresses/", new { id = a.Id, full_name = a.FullName, city = a.City, is_default = a.IsDefault });
    }

    private static async Task<IResult> GetMeAddressDefault(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(x => x.WorkspaceId == wid && x.ObjectId == customerId && x.IsDefault, ct);
        if (a == null) a = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(x => x.WorkspaceId == wid && x.ObjectId == customerId, ct);
        if (a == null) return Results.NotFound();
        return Results.Ok(new { id = a.Id, full_name = a.FullName, city = a.City, is_default = a.IsDefault });
    }

    private static async Task<IResult> GetMeAddress(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid && x.ObjectId == customerId, ct);
        if (a == null) return Results.NotFound();
        return Results.Ok(new { id = a.Id, full_name = a.FullName, city = a.City });
    }

    private static async Task<IResult> PatchMeAddress(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid && x.ObjectId == customerId, ct);
        if (a == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<MeAddressPatchBody>(ct);
        if (body?.city != null) a.City = body.city;
        a.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = a.Id, city = a.City });
    }

    private static async Task<IResult> DeleteMeAddress(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid && x.ObjectId == customerId, ct);
        if (a == null) return Results.NotFound();
        db.Addresses.Remove(a);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> GetMeSettings(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var p = await db.UserPreferences.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
        if (p == null) return Results.Ok(new { email_notifications = true, theme = "light", profile_visibility = "private", notify_promotions = true, items_per_page = 20 });
        return Results.Ok(new { email_notifications = true, theme = "light", profile_visibility = "private", notify_promotions = true, items_per_page = 20 });
    }

    private static async Task<IResult> PatchMeSettings(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userId = GetCurrentUserId(ctx);
        if (!userId.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<MeSettingsBody>(ct);
        return Results.Ok(new { email_notifications = body?.email_notifications ?? true, theme = body?.theme ?? "light", profile_visibility = body?.profile_visibility ?? "private", notify_promotions = body?.notify_promotions ?? true, items_per_page = body?.items_per_page ?? 20 });
    }

    private static async Task<IResult> PutMeSettings(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<MeSettingsBody>(ct);
        return Results.Ok(new { email_notifications = body?.email_notifications ?? true, theme = body?.theme ?? "light", profile_visibility = body?.profile_visibility ?? "private", notify_promotions = body?.notify_promotions ?? true, items_per_page = body?.items_per_page ?? 20, custom_preferences = body?.custom_preferences });
    }

    private static async Task<IResult> ListMeOrders(BfgDbContext db, HttpContext ctx, string? status, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        if (!customerId.HasValue) return Results.Ok(Array.Empty<object>());
        var query = db.Orders.AsNoTracking().Where(o => o.CustomerId == customerId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);
        var list = await query.OrderByDescending(o => o.CreatedAt).Select(o => new { id = o.Id, order_number = o.OrderNumber, status = o.Status, total_amount = o.TotalAmount }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetMeOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var o = await db.Orders.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (o == null) return Results.NotFound();
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, workspace = o.WorkspaceId, customer_id = o.CustomerId, store_id = o.StoreId, status = o.Status, payment_status = o.PaymentStatus, subtotal_amount = o.Subtotal.ToString("F2"), shipping_amount = o.ShippingCost.ToString("F2"), discount_amount = o.Discount.ToString("F2"), total_amount = o.TotalAmount.ToString("F2"), shipping_address_id = o.ShippingAddressId, billing_address_id = o.BillingAddressId, items, created_at = o.CreatedAt, updated_at = o.UpdatedAt });
    }

    private static async Task<IResult> CancelMeOrder(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var o = await db.Orders.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (o == null) return Results.NotFound();
        if (o.Status == "delivered" || o.Status == "cancelled" || o.Status == "refunded")
            return Results.BadRequest(new { detail = $"Order in '{o.Status}' status cannot be cancelled." });
        o.Status = "cancelled";
        o.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        var items = await db.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => new { id = i.Id, product = i.ProductId, variant = i.VariantId, quantity = i.Quantity, unit_price = i.UnitPrice.ToString("F2"), total_price = i.TotalPrice.ToString("F2") }).ToListAsync(ct);
        return Results.Ok(new { id = o.Id, order_number = o.OrderNumber, status = o.Status, payment_status = o.PaymentStatus, items });
    }

    private static async Task<IResult> ListMePaymentMethods(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        if (!customerId.HasValue) return Results.Ok(Array.Empty<object>());
        var list = await db.PaymentMethods.AsNoTracking().Where(pm => pm.CustomerId == customerId).Select(pm => new { id = pm.Id, method_type = pm.MethodType, card_last4 = pm.CardLast4, is_default = pm.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateMePaymentMethod(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!customerId.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentMethodCreateBody>(ct);
        if (body == null || body.gateway <= 0) return Results.BadRequest();
        if (!wid.HasValue || !await db.PaymentGateways.AsNoTracking().AnyAsync(g => g.Id == body.gateway && g.WorkspaceId == wid.Value, ct))
            return Results.BadRequest();
        var mt = body.method_type ?? "card";
        if (mt.Length > 20) mt = mt[..20];
        var brand = (body.card_brand ?? "unknown").ToLowerInvariant();
        if (brand.Length > 20) brand = brand[..20];
        var last4 = body.card_last4 ?? "0000";
        last4 = last4.Length >= 4 ? last4[^4..] : last4.PadLeft(4, '0');
        var disp = body.display_info ?? "";
        if (disp.Length > 255) disp = disp[..255];
        var now = DateTime.UtcNow;
        var pm = new Bfg.Core.Finance.PaymentMethod
        {
            CustomerId = customerId.Value,
            WorkspaceId = wid,
            GatewayId = body.gateway,
            MethodType = mt,
            GatewayToken = "",
            CardholderName = body.cardholder_name ?? "",
            CardBrand = brand,
            CardLast4 = last4,
            CardExpMonth = body.card_exp_month,
            CardExpYear = body.card_exp_year,
            DisplayInfo = disp,
            IsDefault = body.is_default ?? true,
            IsActive = body.is_active ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.PaymentMethods.Add(pm);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/me/payment-methods/", new { id = pm.Id, method_type = pm.MethodType, card_last4 = pm.CardLast4, is_default = pm.IsDefault });
    }

    private static async Task<IResult> GetMePaymentMethod(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var pm = await db.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (pm == null) return Results.NotFound();
        return Results.Ok(new { id = pm.Id, cardholder_name = pm.CardholderName, is_default = pm.IsDefault });
    }

    private static async Task<IResult> PatchMePaymentMethod(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var pm = await db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (pm == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentMethodPatchBody>(ct);
        if (body?.is_default == true) { foreach (var other in await db.PaymentMethods.Where(x => x.CustomerId == customerId).ToListAsync(ct)) other.IsDefault = false; pm.IsDefault = true; }
        if (body?.cardholder_name != null) pm.CardholderName = body.cardholder_name;
        pm.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = pm.Id, is_default = pm.IsDefault, cardholder_name = pm.CardholderName });
    }

    private static async Task<IResult> DeleteMePaymentMethod(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var pm = await db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (pm == null) return Results.NotFound();
        db.PaymentMethods.Remove(pm);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListMePayments(BfgDbContext db, HttpContext ctx, string? status, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        if (!customerId.HasValue) return Results.Ok(Array.Empty<object>());
        var query = db.Payments.AsNoTracking().Where(p => p.CustomerId == customerId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);
        var list = await query.OrderByDescending(p => p.CreatedAt).Select(p => new { id = p.Id, amount = p.Amount.ToString("F2"), status = p.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetMePayment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var p = await db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (p == null) return Results.NotFound();
        var currency = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == p.CurrencyId, ct);
        return Results.Ok(new { id = p.Id, amount = p.Amount.ToString("F2"), currency_code = currency?.Code ?? "" });
    }

    private static async Task<IResult> SendMePayment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var p = await db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { message = "Payment sent" });
    }

    private static async Task<IResult> ListMeInvoices(BfgDbContext db, HttpContext ctx, string? status, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        if (!customerId.HasValue) return Results.Ok(Array.Empty<object>());
        var query = db.Invoices.AsNoTracking().Where(i => i.CustomerId == customerId);
        if (!string.IsNullOrEmpty(status)) query = query.Where(i => i.Status == status);
        var raw = await query.OrderByDescending(i => i.CreatedAt).Select(i => new { i.Id, i.InvoiceNumber, i.TotalAmount, i.Status }).ToListAsync(ct);
        var list = raw.Select(i => (object)new { id = i.Id, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount.ToString("F2"), status = i.Status }).ToList();
        return Results.Ok(list);
    }

    private static async Task<IResult> GetMeInvoice(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var i = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (i == null) return Results.NotFound();
        return Results.Ok(new { id = i.Id, status = i.Status, total = i.TotalAmount.ToString("F2"), items = new List<object>() });
    }

    private static async Task<IResult> DownloadMeInvoicePdf(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var i = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (i == null) return Results.NotFound();
        return Results.Ok(new { url = $"/invoices/{i.InvoiceNumber}.pdf" });
    }

    private static async Task<IResult> SendMeInvoice(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var i = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (i == null) return Results.NotFound();
        return Results.Ok(new { message = "Invoice sent" });
    }

    private static IResult GetMeSupportOptions()
    {
        var options = new[]
        {
            new { type = "chat", available = true },
            new { type = "email", available = true }
        };
        return Results.Ok(options);
    }

    private static async Task<IResult> ListMeTickets(BfgDbContext db, HttpContext ctx, string? status, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        if (!customerId.HasValue) return Results.Ok(Array.Empty<object>());
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.SupportTickets.AsNoTracking().Where(t => t.CustomerId == customerId && (!wid.HasValue || t.WorkspaceId == wid.Value));
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
        var list = await query.OrderByDescending(t => t.CreatedAt)
            .Select(t => new { id = t.Id, subject = t.Subject, status = t.Status, channel = t.Channel, created_at = t.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateMeTicket(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var customerId = await GetCurrentCustomerId(db, ctx, ct);
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!customerId.HasValue || !wid.HasValue) return Results.Unauthorized();
        var body = await ctx.Request.ReadFromJsonAsync<MeTicketCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new SupportTicket
        {
            WorkspaceId = wid.Value,
            CustomerId = customerId.Value,
            TicketNumber = "TKT-" + Guid.NewGuid().ToString("N")[..10].ToUpperInvariant(),
            Subject = body.subject ?? "",
            Description = body.description ?? "",
            Status = "new",
            Channel = string.IsNullOrEmpty(body.channel) ? "web" : body.channel!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.SupportTickets.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/me/tickets/", new { id = t.Id, subject = t.Subject, status = t.Status, customer = t.CustomerId });
    }

    private sealed record ChangePasswordBody(string? old_password, string? new_password, string? confirm_password);
    private sealed record ResetPasswordBody(string? email);
    private sealed record MeAddressCreateBody(string? full_name, string? phone, string? email, string? address_line1, string? address_line2, string? city, string? state, string? postal_code, string? country, bool? is_default);
    private sealed record MeAddressPatchBody(string? city);
    private sealed record MeSettingsBody(bool? email_notifications, string? theme, string? profile_visibility, bool? notify_promotions, int? items_per_page, Dictionary<string, object>? custom_preferences);
    private sealed record PaymentMethodCreateBody(int gateway, string? method_type, string? cardholder_name, string? card_brand, string? card_last4, int? card_exp_month, int? card_exp_year, string? display_info, bool? is_default, bool? is_active);
    private sealed record PaymentMethodPatchBody(bool? is_default, string? cardholder_name);
    private sealed record MePatchBody(string? first_name, string? last_name, string? phone);
    private sealed record AvatarBody(string? avatar_url);
    private sealed record MeTicketCreateBody(string? subject, string? description, string? channel);
}
