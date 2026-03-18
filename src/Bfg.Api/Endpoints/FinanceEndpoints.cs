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
        var g = new PaymentGateway { WorkspaceId = wid.Value, Name = body.name ?? "", GatewayType = body.gateway_type ?? "custom", IsActive = body.is_active ?? true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
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
        // Support both field name styles: order/order_id, gateway/gateway_id
        var orderId = body.order > 0 ? body.order : body.order_id;
        var gatewayId = body.gateway > 0 ? body.gateway : body.gateway_id;
        if (orderId <= 0) return Results.BadRequest(new { order = new[] { "This field is required." } });
        var p = new Payment
        {
            WorkspaceId = wid.Value,
            OrderId = orderId,
            GatewayId = gatewayId > 0 ? gatewayId : (int?)null,
            CurrencyId = body.currency_id > 0 ? body.currency_id : null,
            Amount = decimal.TryParse(body.amount, out var amt) ? amt : 0,
            Status = body.status ?? "pending",
            PaymentNumber = "PAY-" + Guid.NewGuid().ToString("N")[..8],
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
        var list = await query.OrderByDescending(i => i.CreatedAt).Select(i => new { id = i.Id, order_id = i.OrderId, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status, issued_at = i.IssuedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateInvoice(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<InvoiceCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var i = new Invoice
        {
            WorkspaceId = wid.Value,
            OrderId = body.order_id,
            CustomerId = body.customer_id,
            InvoiceNumber = body.invoice_number ?? ("INV-" + Guid.NewGuid().ToString("N")[..8]),
            TotalAmount = decimal.TryParse(body.total_amount, out var t) ? t : 0,
            Status = body.status ?? "draft",
            IssuedAt = body.issued_at,
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
        return Results.Ok(new { id = i.Id, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status, issued_at = i.IssuedAt });
    }

    private sealed record CurrencyCreateBody(string? code, string? name, string? symbol, int? decimal_places);
    private sealed record PaymentGatewayCreateBody(string? name, string? gateway_type, bool? is_active);
    private sealed record PaymentCreateBody(int order_id, int gateway_id, int currency_id, int order, int gateway, string? currency, string? amount, string? status);
    private sealed record InvoiceCreateBody(int? order_id, int? customer_id, string? invoice_number, string? total_amount, string? status, DateTime? issued_at);
}
