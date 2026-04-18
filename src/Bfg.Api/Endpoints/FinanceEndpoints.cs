using System.Text.Json;
using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Finance;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class FinanceEndpoints
{
    public static void MapFinanceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/finance").WithTags("Finance").RequireAuthorization();

        group.MapGet("/currencies", ListCurrencies);
        group.MapPost("/currencies/", CreateCurrency);
        group.MapGet("/currencies/{id:int}", GetCurrency);

        group.MapGet("/payment-gateways", ListPaymentGateways);
        group.MapPost("/payment-gateways/", CreatePaymentGateway);
        group.MapGet("/payment-gateways/{id:int}", GetPaymentGateway);

        group.MapGet("/payments", ListPayments);
        group.MapPost("/payments/", CreatePayment);
        group.MapGet("/payments/{id:int}", GetPayment);

        group.MapGet("/invoices", ListInvoices);
        group.MapPost("/invoices/", CreateInvoice);
        group.MapGet("/invoices/{id:int}", GetInvoice);
    }

    private static async Task<IResult> ListCurrencies(BfgDbContext db, HttpRequest req, CancellationToken ct)
    {
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Currencies.AsNoTracking().Where(c => c.IsActive);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(c => c.Code).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new { id = c.Id, code = c.Code, name = c.Name, symbol = c.Symbol, decimal_places = c.DecimalPlaces, is_active = c.IsActive })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateCurrency(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<CurrencyCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var c = new Currency { Code = body.code ?? "", Name = body.name ?? "", Symbol = body.symbol ?? "", DecimalPlaces = body.decimal_places ?? 2, IsActive = true };
        db.Currencies.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/currencies/", new { id = c.Id, code = c.Code, name = c.Name, symbol = c.Symbol });
    }

    private static async Task<IResult> GetCurrency(BfgDbContext db, int id, CancellationToken ct)
    {
        var c = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, code = c.Code, name = c.Name, symbol = c.Symbol, is_active = c.IsActive });
    }

    private static async Task<IResult> ListPaymentGateways(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.PaymentGateways.AsNoTracking().Where(g => !wid.HasValue || g.WorkspaceId == wid.Value);
        var list = await query.Select(g => new { id = g.Id, name = g.Name, gateway_type = g.GatewayType, is_active = g.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreatePaymentGateway(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentGatewayCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var gwType = body.gateway_type ?? "custom";
        if (gwType.Length > 20) gwType = gwType[..20];
        var g = new PaymentGateway
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            GatewayType = gwType,
            Config = "{}",
            TestConfig = "{}",
            IsActive = body.is_active ?? true,
            IsTestMode = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.PaymentGateways.Add(g);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/payment-gateways/", new { id = g.Id, name = g.Name, gateway_type = g.GatewayType, is_active = g.IsActive });
    }

    private static async Task<IResult> GetPaymentGateway(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var g = await db.PaymentGateways.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (g == null) return Results.NotFound();
        return Results.Ok(new { id = g.Id, name = g.Name, gateway_type = g.GatewayType });
    }

    private static async Task<IResult> ListPayments(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Payments.AsNoTracking().Where(p => !wid.HasValue || p.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(p => p.CreatedAt).Select(p => new { id = p.Id, order_id = p.OrderId, amount = p.Amount, status = p.Status, created_at = p.CreatedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreatePayment(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var orderId = body.order > 0 ? body.order : body.order_id;
        var gatewayId = body.gateway > 0 ? body.gateway : body.gateway_id;
        if (orderId <= 0) return Results.BadRequest(new { order = new[] { "This field is required." } });
        if (body.currency_id <= 0) return Results.BadRequest(new { currency_id = new[] { "This field is required." } });

        var order = await db.Orders.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId && o.WorkspaceId == wid.Value, ct);
        if (order == null) return Results.NotFound();

        var displayName = "Manual";
        var gatewayType = "custom";
        if (gatewayId > 0)
        {
            var gw = await db.PaymentGateways.AsNoTracking()
                .FirstOrDefaultAsync(g => g.Id == gatewayId && g.WorkspaceId == wid.Value, ct);
            if (gw != null)
            {
                displayName = string.IsNullOrEmpty(gw.Name) ? "Gateway" : gw.Name;
                gatewayType = string.IsNullOrEmpty(gw.GatewayType) ? "custom" : gw.GatewayType;
            }
        }
        if (gatewayType.Length > 20) gatewayType = gatewayType[..20];

        var payNum = "PAY-" + Guid.NewGuid().ToString("N")[..12];
        if (payNum.Length > 50) payNum = payNum[..50];
        var payStatus = body.status ?? "pending";
        if (payStatus.Length > 20) payStatus = payStatus[..20];

        var p = new Payment
        {
            WorkspaceId = wid.Value,
            OrderId = orderId,
            CustomerId = order.CustomerId,
            GatewayId = gatewayId > 0 ? gatewayId : null,
            CurrencyId = body.currency_id,
            Amount = decimal.TryParse(body.amount, out var amt) ? amt : 0,
            Status = payStatus,
            PaymentNumber = payNum,
            GatewayDisplayName = displayName,
            GatewayType = gatewayType,
            GatewayTransactionId = "",
            GatewayResponse = "{}",
            CreatedAt = DateTime.UtcNow
        };
        db.Payments.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/payments/", new { id = p.Id, order_id = p.OrderId, amount = p.Amount.ToString("F2"), status = p.Status });
    }

    private static async Task<IResult> GetPayment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { id = p.Id, amount = p.Amount.ToString("F2"), status = p.Status, created_at = p.CreatedAt });
    }

    private static async Task<IResult> ListInvoices(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Invoices.AsNoTracking().Where(i => !wid.HasValue || i.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(i => i.CreatedAt).Select(i => new { id = i.Id, order_id = i.OrderId, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status, issued_at = i.IssueDate }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateInvoice(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<InvoiceCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var customerId = body.customer ?? body.customer_id;
        if (customerId is null or <= 0)
            return Results.BadRequest(new { customer = new[] { "This field is required." } });

        if (body.items.HasValue && body.items.Value.ValueKind == JsonValueKind.Array)
        {
            foreach (var el in body.items.Value.EnumerateArray())
            {
                decimal qty = 1;
                if (el.TryGetProperty("quantity", out var qEl))
                {
                    if (qEl.ValueKind == JsonValueKind.Number && qEl.TryGetDecimal(out var qd))
                        qty = qd;
                    else if (qEl.ValueKind == JsonValueKind.String && decimal.TryParse(qEl.GetString(), out var qs))
                        qty = qs;
                }
                if (qty < 0)
                    return Results.BadRequest(new { detail = "quantity must be non-negative" });
                var unitOk = false;
                decimal unitPrice = 0;
                if (el.TryGetProperty("unit_price", out var upEl))
                {
                    if (upEl.ValueKind == JsonValueKind.String && decimal.TryParse(upEl.GetString(), out unitPrice))
                        unitOk = true;
                    else if (upEl.ValueKind == JsonValueKind.Number && upEl.TryGetDecimal(out unitPrice))
                        unitOk = true;
                }
                if (unitOk && unitPrice < 0)
                    return Results.BadRequest(new { detail = "unit price must be non-negative" });
                if (el.TryGetProperty("discount", out var dEl))
                {
                    var dStr = dEl.ValueKind == JsonValueKind.String ? dEl.GetString() : dEl.GetRawText();
                    if (decimal.TryParse(dStr, out var dMul) && dMul > 1)
                        return Results.BadRequest(new { discount = new[] { "Discount multiplier cannot exceed 1." } });
                }
            }
        }

        var currencyId = await db.Currencies.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Id).Select(c => c.Id).FirstOrDefaultAsync(ct);
        if (currencyId == 0)
            return Results.BadRequest(new { detail = "No active currency configured." });

        var parsedTotal = decimal.TryParse(body.total_amount, out var t) ? t : 0;
        var today = DateTime.UtcNow.Date;
        var due = today.AddDays(30);
        var i = new Invoice
        {
            WorkspaceId = wid.Value,
            OrderId = body.order_id,
            CustomerId = customerId.Value,
            InvoiceNumber = body.invoice_number ?? ("INV-" + Guid.NewGuid().ToString("N")[..8]),
            Subtotal = parsedTotal,
            Tax = 0,
            TotalAmount = parsedTotal,
            CurrencyId = currencyId,
            Status = body.status ?? "draft",
            IssueDate = today,
            DueDate = due,
            Notes = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Invoices.Add(i);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/invoices/", new { id = i.Id, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status });
    }

    private static async Task<IResult> GetInvoice(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var i = await db.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (i == null) return Results.NotFound();
        return Results.Ok(new { id = i.Id, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status, issued_at = i.IssueDate });
    }

    private sealed record CurrencyCreateBody(string? code, string? name, string? symbol, int? decimal_places);
    private sealed record PaymentGatewayCreateBody(string? name, string? gateway_type, bool? is_active);
    private sealed record PaymentCreateBody(int order_id, int gateway_id, int currency_id, int order, int gateway, string? currency, string? amount, string? status);

    private sealed class InvoiceCreateBody
    {
        public int? order_id { get; set; }
        public int? customer { get; set; }
        public int? customer_id { get; set; }
        public string? invoice_number { get; set; }
        public string? total_amount { get; set; }
        public string? status { get; set; }
        public JsonElement? items { get; set; }
    }
}
