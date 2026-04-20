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
        var root = app.MapGroup("/api/v1").WithTags("Finance").RequireAuthorization();

        // Currencies
        group.MapGet("/currencies", ListCurrencies);
        group.MapPost("/currencies/", CreateCurrency);
        group.MapGet("/currencies/default-code", GetDefaultCurrencyCode);
        group.MapGet("/currencies/{id:int}", GetCurrency);
        group.MapPatch("/currencies/{id:int}", PatchCurrency);
        group.MapDelete("/currencies/{id:int}", DeleteCurrency);

        root.MapGet("/currencies", ListCurrencies);
        root.MapPost("/currencies/", CreateCurrency);
        root.MapGet("/currencies/default-code", GetDefaultCurrencyCode);
        root.MapGet("/currencies/{id:int}", GetCurrency);
        root.MapPatch("/currencies/{id:int}", PatchCurrency);
        root.MapDelete("/currencies/{id:int}", DeleteCurrency);

        // Payment Gateways
        group.MapGet("/payment-gateways", ListPaymentGateways);
        group.MapPost("/payment-gateways/", CreatePaymentGateway);
        group.MapGet("/payment-gateways/plugins", ListPaymentGatewayPlugins);
        group.MapGet("/payment-gateways/{id:int}", GetPaymentGateway);
        group.MapPatch("/payment-gateways/{id:int}", PatchPaymentGateway);
        group.MapDelete("/payment-gateways/{id:int}", DeletePaymentGateway);

        root.MapGet("/payment-gateways", ListPaymentGateways);
        root.MapPost("/payment-gateways/", CreatePaymentGateway);
        root.MapGet("/payment-gateways/plugins", ListPaymentGatewayPlugins);
        root.MapGet("/payment-gateways/{id:int}", GetPaymentGateway);
        root.MapPatch("/payment-gateways/{id:int}", PatchPaymentGateway);
        root.MapDelete("/payment-gateways/{id:int}", DeletePaymentGateway);

        // Payments
        group.MapGet("/payments", ListPayments);
        group.MapPost("/payments/", CreatePayment);
        group.MapGet("/payments/{id:int}", GetPayment);
        group.MapPatch("/payments/{id:int}", PatchPayment);
        group.MapDelete("/payments/{id:int}", DeletePayment);

        root.MapGet("/payments", ListPayments);
        root.MapPost("/payments/", CreatePayment);
        root.MapGet("/payments/{id:int}", GetPayment);
        root.MapPatch("/payments/{id:int}", PatchPayment);
        root.MapDelete("/payments/{id:int}", DeletePayment);

        // Invoices
        group.MapGet("/invoices", ListInvoices);
        group.MapPost("/invoices/", CreateInvoice);
        group.MapGet("/invoices/{id:int}", GetInvoice);
        group.MapPatch("/invoices/{id:int}", PatchInvoice);
        group.MapDelete("/invoices/{id:int}", DeleteInvoice);
        group.MapPost("/invoices/{id:int}/send", SendInvoice);
        group.MapPost("/invoices/{id:int}/update_items", UpdateInvoiceItems);

        root.MapGet("/invoices", ListInvoices);
        root.MapPost("/invoices/", CreateInvoice);
        root.MapGet("/invoices/{id:int}", GetInvoice);
        root.MapPatch("/invoices/{id:int}", PatchInvoice);
        root.MapDelete("/invoices/{id:int}", DeleteInvoice);
        root.MapPost("/invoices/{id:int}/send", SendInvoice);
        root.MapPost("/invoices/{id:int}/update_items", UpdateInvoiceItems);

        // Invoice Items
        group.MapGet("/invoice-items", ListInvoiceItems);
        group.MapPost("/invoice-items/", CreateInvoiceItem);

        root.MapGet("/invoice-items", ListInvoiceItems);
        root.MapPost("/invoice-items/", CreateInvoiceItem);

        // Brands
        group.MapGet("/brands", ListBrands);
        group.MapPost("/brands/", CreateBrand);
        group.MapGet("/brands/{id:int}", GetBrand);
        group.MapPatch("/brands/{id:int}", PatchBrand);
        group.MapDelete("/brands/{id:int}", DeleteBrand);

        root.MapGet("/brands", ListBrands);
        root.MapPost("/brands/", CreateBrand);
        root.MapGet("/brands/{id:int}", GetBrand);
        root.MapPatch("/brands/{id:int}", PatchBrand);
        root.MapDelete("/brands/{id:int}", DeleteBrand);

        // Financial Codes
        group.MapGet("/financial-codes", ListFinancialCodes);
        group.MapPost("/financial-codes/", CreateFinancialCode);
        group.MapGet("/financial-codes/{id:int}", GetFinancialCode);
        group.MapPatch("/financial-codes/{id:int}", PatchFinancialCode);
        group.MapDelete("/financial-codes/{id:int}", DeleteFinancialCode);

        root.MapGet("/financial-codes", ListFinancialCodes);
        root.MapPost("/financial-codes/", CreateFinancialCode);
        root.MapGet("/financial-codes/{id:int}", GetFinancialCode);
        root.MapPatch("/financial-codes/{id:int}", PatchFinancialCode);
        root.MapDelete("/financial-codes/{id:int}", DeleteFinancialCode);

        // Tax Rates
        group.MapGet("/tax-rates", ListTaxRates);
        group.MapPost("/tax-rates/", CreateTaxRate);
        group.MapGet("/tax-rates/{id:int}", GetTaxRate);
        group.MapPatch("/tax-rates/{id:int}", PatchTaxRate);
        group.MapDelete("/tax-rates/{id:int}", DeleteTaxRate);

        root.MapGet("/tax-rates", ListTaxRates);
        root.MapPost("/tax-rates/", CreateTaxRate);
        root.MapGet("/tax-rates/{id:int}", GetTaxRate);
        root.MapPatch("/tax-rates/{id:int}", PatchTaxRate);
        root.MapDelete("/tax-rates/{id:int}", DeleteTaxRate);

        // Wallets
        group.MapGet("/wallets", ListWallets);
        group.MapGet("/wallets/{id:int}", GetWallet);
        group.MapPost("/wallets/{id:int}/withdraw", WalletWithdraw);

        root.MapGet("/wallets", ListWallets);
        root.MapGet("/wallets/{id:int}", GetWallet);
        root.MapPost("/wallets/{id:int}/withdraw", WalletWithdraw);

        // Transactions
        group.MapGet("/transactions", ListTransactions);
        group.MapGet("/transactions/{id:int}", GetTransaction);

        root.MapGet("/transactions", ListTransactions);
        root.MapGet("/transactions/{id:int}", GetTransaction);

        // Withdrawal Requests
        group.MapGet("/withdrawal-requests", ListWithdrawalRequests);
        group.MapGet("/withdrawal-requests/{id:int}", GetWithdrawalRequest);
        group.MapPost("/withdrawal-requests/{id:int}/approve", ApproveWithdrawal);
        group.MapPost("/withdrawal-requests/{id:int}/reject", RejectWithdrawal);

        root.MapGet("/withdrawal-requests", ListWithdrawalRequests);
        root.MapGet("/withdrawal-requests/{id:int}", GetWithdrawalRequest);
        root.MapPost("/withdrawal-requests/{id:int}/approve", ApproveWithdrawal);
        root.MapPost("/withdrawal-requests/{id:int}/reject", RejectWithdrawal);

        // Payment Methods
        group.MapGet("/payment-methods", ListPaymentMethods);
        group.MapPost("/payment-methods/", CreatePaymentMethod);
        group.MapGet("/payment-methods/{id:int}", GetPaymentMethod);
        group.MapPatch("/payment-methods/{id:int}", PatchPaymentMethod);
        group.MapDelete("/payment-methods/{id:int}", DeletePaymentMethod);

        root.MapGet("/payment-methods", ListPaymentMethods);
        root.MapPost("/payment-methods/", CreatePaymentMethod);
        root.MapGet("/payment-methods/{id:int}", GetPaymentMethod);
        root.MapPatch("/payment-methods/{id:int}", PatchPaymentMethod);
        root.MapDelete("/payment-methods/{id:int}", DeletePaymentMethod);
    }

    // -------------------------------------------------------------------------
    // Currencies
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListCurrencies(BfgDbContext db, HttpRequest req, CancellationToken ct)
    {
        if (!await db.Currencies.AsNoTracking().AnyAsync(c => c.IsActive, ct))
        {
            db.Currencies.AddRange(
                new Currency { Code = "USD", Name = "US Dollar", Symbol = "$", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "EUR", Name = "Euro", Symbol = "€", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "GBP", Name = "British Pound", Symbol = "£", DecimalPlaces = 2, IsActive = true },
                new Currency { Code = "CNY", Name = "Chinese Yuan", Symbol = "¥", DecimalPlaces = 2, IsActive = true }
            );
            await db.SaveChangesAsync(ct);
        }

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

    private static async Task<IResult> GetDefaultCurrencyCode(BfgDbContext db, CancellationToken ct)
    {
        var code = await db.Currencies.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Id).Select(c => c.Code).FirstOrDefaultAsync(ct);
        if (code == null) return Results.NotFound();
        return Results.Ok(new { code });
    }

    private static async Task<IResult> GetCurrency(BfgDbContext db, int id, CancellationToken ct)
    {
        var c = await db.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, code = c.Code, name = c.Name, symbol = c.Symbol, is_active = c.IsActive });
    }

    private static async Task<IResult> PatchCurrency(BfgDbContext db, int id, HttpContext ctx, CancellationToken ct)
    {
        var c = await db.Currencies.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CurrencyPatchBody>(ct);
        if (body != null)
        {
            if (body.code != null) c.Code = body.code;
            if (body.name != null) c.Name = body.name;
            if (body.symbol != null) c.Symbol = body.symbol;
            if (body.decimal_places.HasValue) c.DecimalPlaces = body.decimal_places.Value;
            if (body.is_active.HasValue) c.IsActive = body.is_active.Value;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = c.Id, code = c.Code, name = c.Name, symbol = c.Symbol, decimal_places = c.DecimalPlaces, is_active = c.IsActive });
    }

    private static async Task<IResult> DeleteCurrency(BfgDbContext db, int id, CancellationToken ct)
    {
        var c = await db.Currencies.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return Results.NotFound();
        c.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Payment Gateways
    // -------------------------------------------------------------------------

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

    private static IResult ListPaymentGatewayPlugins()
    {
        var plugins = new[]
        {
            new { type = "stripe",        label = "Stripe" },
            new { type = "paypal",        label = "PayPal" },
            new { type = "wechat",        label = "WeChat Pay" },
            new { type = "alipay",        label = "Alipay" },
            new { type = "bank_transfer", label = "Bank Transfer" },
            new { type = "custom",        label = "Custom" }
        };
        return Results.Ok(plugins);
    }

    private static async Task<IResult> GetPaymentGateway(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var g = await db.PaymentGateways.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (g == null) return Results.NotFound();
        return Results.Ok(new { id = g.Id, name = g.Name, gateway_type = g.GatewayType });
    }

    private static async Task<IResult> PatchPaymentGateway(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var g = await db.PaymentGateways.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (g == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentGatewayPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) g.Name = body.name;
            if (body.gateway_type != null) { var t = body.gateway_type; if (t.Length > 20) t = t[..20]; g.GatewayType = t; }
            if (body.is_active.HasValue) g.IsActive = body.is_active.Value;
            g.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = g.Id, name = g.Name, gateway_type = g.GatewayType, is_active = g.IsActive });
    }

    private static async Task<IResult> DeletePaymentGateway(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var g = await db.PaymentGateways.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (g == null) return Results.NotFound();
        db.PaymentGateways.Remove(g);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Payments
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListPayments(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Payments.AsNoTracking().Where(p => !wid.HasValue || p.WorkspaceId == wid.Value);
        var raw = await query.OrderByDescending(p => p.CreatedAt).Select(p => new { p.Id, p.OrderId, p.Amount, p.Status, p.CreatedAt }).ToListAsync(ct);
        var list = raw.Select(p => (object)new { id = p.Id, order_id = p.OrderId, amount = p.Amount.ToString("F2"), status = p.Status, created_at = p.CreatedAt }).ToList();
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

    private static async Task<IResult> PatchPayment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentPatchBody>(ct);
        if (body?.status != null)
        {
            var s = body.status;
            if (s.Length > 20) s = s[..20];
            p.Status = s;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = p.Id, order_id = p.OrderId, amount = p.Amount.ToString("F2"), status = p.Status, created_at = p.CreatedAt });
    }

    private static async Task<IResult> DeletePayment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.Payments.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        db.Payments.Remove(p);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Invoices
    // -------------------------------------------------------------------------

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

    private static async Task<IResult> PatchInvoice(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var i = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (i == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<InvoicePatchBody>(ct);
        if (body != null)
        {
            if (body.status != null) i.Status = body.status;
            if (body.notes != null) i.Notes = body.notes;
            if (body.due_date.HasValue) i.DueDate = body.due_date.Value;
            i.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = i.Id, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status, notes = i.Notes, due_date = i.DueDate, issued_at = i.IssueDate });
    }

    private static async Task<IResult> DeleteInvoice(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var i = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (i == null) return Results.NotFound();
        db.Invoices.Remove(i);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SendInvoice(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var i = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (i == null) return Results.NotFound();
        i.Status = "sent";
        i.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = i.Id, invoice_number = i.InvoiceNumber, total_amount = i.TotalAmount, status = i.Status, issued_at = i.IssueDate });
    }

    private static async Task<IResult> UpdateInvoiceItems(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var invoice = await db.Invoices.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (invoice == null) return Results.NotFound();

        var body = await ctx.Request.ReadFromJsonAsync<UpdateInvoiceItemsBody>(ct);
        if (body?.items == null) return Results.BadRequest(new { detail = "items is required." });

        // Remove existing items and replace
        var existing = await db.InvoiceItems.Where(x => x.InvoiceId == id).ToListAsync(ct);
        db.InvoiceItems.RemoveRange(existing);

        decimal subtotal = 0;
        foreach (var item in body.items)
        {
            var qty = item.quantity ?? 1m;
            var unitPrice = item.unit_price ?? 0m;
            var discount = item.discount ?? 1m;
            var lineSubtotal = qty * unitPrice * discount;
            var lineTax = item.tax ?? 0m;
            subtotal += lineSubtotal;
            db.InvoiceItems.Add(new InvoiceItem
            {
                InvoiceId = id,
                Description = item.description ?? "",
                Quantity = qty,
                UnitPrice = unitPrice,
                Discount = discount,
                Subtotal = lineSubtotal,
                Tax = lineTax,
                TaxType = item.tax_type ?? "",
                FinancialCodeId = item.financial_code_id,
                ProductId = item.product_id
            });
        }

        invoice.Subtotal = subtotal;
        invoice.TotalAmount = subtotal + (body.items.Sum(x => x.tax ?? 0m));
        invoice.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { id = invoice.Id, invoice_number = invoice.InvoiceNumber, total_amount = invoice.TotalAmount, status = invoice.Status });
    }

    // -------------------------------------------------------------------------
    // Invoice Items
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListInvoiceItems(BfgDbContext db, HttpRequest req, CancellationToken ct)
    {
        var query = db.InvoiceItems.AsNoTracking();
        if (int.TryParse(req.Query["invoice_id"], out var invoiceId) && invoiceId > 0)
            query = query.Where(x => x.InvoiceId == invoiceId);
        var list = await query.OrderBy(x => x.Id)
            .Select(x => new { id = x.Id, invoice_id = x.InvoiceId, description = x.Description, quantity = x.Quantity, unit_price = x.UnitPrice, discount = x.Discount, subtotal = x.Subtotal, tax = x.Tax, tax_type = x.TaxType, financial_code_id = x.FinancialCodeId, product_id = x.ProductId })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateInvoiceItem(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<InvoiceItemCreateBody>(ct);
        if (body == null || body.invoice_id <= 0) return Results.BadRequest(new { invoice_id = new[] { "This field is required." } });
        var qty = body.quantity ?? 1m;
        var unitPrice = body.unit_price ?? 0m;
        var discount = body.discount ?? 1m;
        var lineSubtotal = qty * unitPrice * discount;
        var item = new InvoiceItem
        {
            InvoiceId = body.invoice_id,
            Description = body.description ?? "",
            Quantity = qty,
            UnitPrice = unitPrice,
            Discount = discount,
            Subtotal = lineSubtotal,
            Tax = body.tax ?? 0m,
            TaxType = body.tax_type ?? "",
            FinancialCodeId = body.financial_code_id,
            ProductId = body.product_id
        };
        db.InvoiceItems.Add(item);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/invoice-items/", new { id = item.Id, invoice_id = item.InvoiceId, description = item.Description, quantity = item.Quantity, unit_price = item.UnitPrice, subtotal = item.Subtotal });
    }

    // -------------------------------------------------------------------------
    // Brands
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListBrands(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Brands.AsNoTracking().Where(b => !wid.HasValue || b.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(b => b.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(b => new { id = b.Id, workspace_id = b.WorkspaceId, name = b.Name, logo = b.Logo, is_default = b.IsDefault, tax_id = b.TaxId, registration_number = b.RegistrationNumber })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateBrand(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<BrandCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var b = new Brand
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Logo = body.logo ?? "",
            IsDefault = body.is_default ?? false,
            TaxId = body.tax_id ?? "",
            RegistrationNumber = body.registration_number ?? "",
            InvoiceNote = body.invoice_note ?? "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Brands.Add(b);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/brands/", new { id = b.Id, name = b.Name, is_default = b.IsDefault });
    }

    private static async Task<IResult> GetBrand(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Brands.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        return Results.Ok(new { id = b.Id, workspace_id = b.WorkspaceId, name = b.Name, logo = b.Logo, is_default = b.IsDefault, tax_id = b.TaxId, registration_number = b.RegistrationNumber, invoice_note = b.InvoiceNote });
    }

    private static async Task<IResult> PatchBrand(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Brands.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<BrandPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) b.Name = body.name;
            if (body.logo != null) b.Logo = body.logo;
            if (body.is_default.HasValue) b.IsDefault = body.is_default.Value;
            if (body.tax_id != null) b.TaxId = body.tax_id;
            if (body.registration_number != null) b.RegistrationNumber = body.registration_number;
            if (body.invoice_note != null) b.InvoiceNote = body.invoice_note;
            b.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = b.Id, name = b.Name, logo = b.Logo, is_default = b.IsDefault, tax_id = b.TaxId, registration_number = b.RegistrationNumber, invoice_note = b.InvoiceNote });
    }

    private static async Task<IResult> DeleteBrand(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var b = await db.Brands.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (b == null) return Results.NotFound();
        db.Brands.Remove(b);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Financial Codes
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListFinancialCodes(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.FinancialCodes.AsNoTracking().Where(f => (!wid.HasValue || f.WorkspaceId == wid.Value) && f.IsActive);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(f => f.Code).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new { id = f.Id, code = f.Code, name = f.Name, description = f.Description, unit_price = f.UnitPrice, unit = f.Unit, tax_type = f.TaxType, is_active = f.IsActive })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateFinancialCode(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<FinancialCodeCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var f = new FinancialCode
        {
            WorkspaceId = wid.Value,
            Code = body.code ?? "",
            Name = body.name ?? "",
            Description = body.description ?? "",
            UnitPrice = body.unit_price ?? 0m,
            Unit = body.unit ?? "",
            TaxType = body.tax_type ?? "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.FinancialCodes.Add(f);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/financial-codes/", new { id = f.Id, code = f.Code, name = f.Name });
    }

    private static async Task<IResult> GetFinancialCode(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FinancialCodes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        return Results.Ok(new { id = f.Id, code = f.Code, name = f.Name, description = f.Description, unit_price = f.UnitPrice, unit = f.Unit, tax_type = f.TaxType, is_active = f.IsActive });
    }

    private static async Task<IResult> PatchFinancialCode(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FinancialCodes.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<FinancialCodePatchBody>(ct);
        if (body != null)
        {
            if (body.code != null) f.Code = body.code;
            if (body.name != null) f.Name = body.name;
            if (body.description != null) f.Description = body.description;
            if (body.unit_price.HasValue) f.UnitPrice = body.unit_price.Value;
            if (body.unit != null) f.Unit = body.unit;
            if (body.tax_type != null) f.TaxType = body.tax_type;
            if (body.is_active.HasValue) f.IsActive = body.is_active.Value;
            f.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = f.Id, code = f.Code, name = f.Name, description = f.Description, unit_price = f.UnitPrice, unit = f.Unit, tax_type = f.TaxType, is_active = f.IsActive });
    }

    private static async Task<IResult> DeleteFinancialCode(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FinancialCodes.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        db.FinancialCodes.Remove(f);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Tax Rates
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListTaxRates(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.TaxRates.AsNoTracking().Where(t => (!wid.HasValue || t.WorkspaceId == wid.Value) && t.IsActive);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(t => t.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new { id = t.Id, name = t.Name, rate = t.Rate, country = t.Country, state = t.State, is_active = t.IsActive })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateTaxRate(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TaxRateCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new TaxRate
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Rate = body.rate ?? 0m,
            Country = body.country ?? "",
            State = body.state ?? "",
            IsActive = true
        };
        db.TaxRates.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/tax-rates/", new { id = t.Id, name = t.Name, rate = t.Rate });
    }

    private static async Task<IResult> GetTaxRate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.TaxRates.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, name = t.Name, rate = t.Rate, country = t.Country, state = t.State, is_active = t.IsActive });
    }

    private static async Task<IResult> PatchTaxRate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.TaxRates.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TaxRatePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) t.Name = body.name;
            if (body.rate.HasValue) t.Rate = body.rate.Value;
            if (body.country != null) t.Country = body.country;
            if (body.state != null) t.State = body.state;
            if (body.is_active.HasValue) t.IsActive = body.is_active.Value;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = t.Id, name = t.Name, rate = t.Rate, country = t.Country, state = t.State, is_active = t.IsActive });
    }

    private static async Task<IResult> DeleteTaxRate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.TaxRates.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        db.TaxRates.Remove(t);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Wallets
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListWallets(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Wallets.AsNoTracking().Where(w => !wid.HasValue || w.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(w => w.Id).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(w => new { id = w.Id, workspace_id = w.WorkspaceId, customer_id = w.CustomerId, cash_balance = w.CashBalance, credit_balance = w.CreditBalance, currency_id = w.CurrencyId, credit_limit = w.CreditLimit, updated_at = w.UpdatedAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> GetWallet(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var w = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (w == null) return Results.NotFound();
        return Results.Ok(new { id = w.Id, workspace_id = w.WorkspaceId, customer_id = w.CustomerId, cash_balance = w.CashBalance, credit_balance = w.CreditBalance, currency_id = w.CurrencyId, credit_limit = w.CreditLimit, updated_at = w.UpdatedAt });
    }

    private static async Task<IResult> WalletWithdraw(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var wallet = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (wallet == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<WalletWithdrawBody>(ct);
        if (body == null || body.amount <= 0)
            return Results.BadRequest(new { amount = new[] { "A positive amount is required." } });
        if (wallet.CashBalance < body.amount)
            return Results.BadRequest(new { detail = "Insufficient balance." });

        var wr = new WithdrawalRequest
        {
            WalletId = id,
            Amount = body.amount,
            Status = "pending",
            PayoutMethod = body.payout_method ?? "",
            PayoutDetails = body.payout_details ?? "",
            Notes = body.notes ?? "",
            RejectionReason = "",
            RequestedAt = DateTime.UtcNow
        };
        db.WithdrawalRequests.Add(wr);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/withdrawal-requests/", new { id = wr.Id, wallet_id = wr.WalletId, amount = wr.Amount, status = wr.Status, requested_at = wr.RequestedAt });
    }

    // -------------------------------------------------------------------------
    // Transactions
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListTransactions(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Transactions.AsNoTracking().Where(t => !wid.HasValue || t.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending(t => t.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new { id = t.Id, workspace_id = t.WorkspaceId, customer_id = t.CustomerId, transaction_type = t.TransactionType, amount = t.Amount, currency_id = t.CurrencyId, wallet_id = t.WalletId, balance_type = t.BalanceType, tx_status = t.TxStatus, description = t.Description, created_at = t.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> GetTransaction(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.Transactions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, workspace_id = t.WorkspaceId, customer_id = t.CustomerId, transaction_type = t.TransactionType, amount = t.Amount, currency_id = t.CurrencyId, wallet_id = t.WalletId, balance_type = t.BalanceType, tx_status = t.TxStatus, source_type = t.SourceType, source_id = t.SourceId, payment_id = t.PaymentId, invoice_id = t.InvoiceId, description = t.Description, notes = t.Notes, created_at = t.CreatedAt });
    }

    // -------------------------------------------------------------------------
    // Withdrawal Requests
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListWithdrawalRequests(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        // Filter via wallet → workspace
        IQueryable<WithdrawalRequest> query = db.WithdrawalRequests.AsNoTracking();
        if (wid.HasValue)
        {
            var walletIds = await db.Wallets.AsNoTracking().Where(w => w.WorkspaceId == wid.Value).Select(w => w.Id).ToListAsync(ct);
            query = query.Where(r => walletIds.Contains(r.WalletId));
        }
        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending(r => r.RequestedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(r => new { id = r.Id, wallet_id = r.WalletId, amount = r.Amount, status = r.Status, payout_method = r.PayoutMethod, requested_at = r.RequestedAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> GetWithdrawalRequest(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var r = await db.WithdrawalRequests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r == null) return Results.NotFound();
        if (wid.HasValue)
        {
            var wallet = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == r.WalletId && w.WorkspaceId == wid.Value, ct);
            if (wallet == null) return Results.NotFound();
        }
        return Results.Ok(new { id = r.Id, wallet_id = r.WalletId, transaction_id = r.TransactionId, amount = r.Amount, status = r.Status, payout_method = r.PayoutMethod, payout_details = r.PayoutDetails, notes = r.Notes, rejection_reason = r.RejectionReason, requested_at = r.RequestedAt, approved_at = r.ApprovedAt, completed_at = r.CompletedAt });
    }

    private static async Task<IResult> ApproveWithdrawal(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var r = await db.WithdrawalRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r == null) return Results.NotFound();
        if (wid.HasValue)
        {
            var wallet = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == r.WalletId && w.WorkspaceId == wid.Value, ct);
            if (wallet == null) return Results.NotFound();
        }
        r.Status = "approved";
        r.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = r.Id, wallet_id = r.WalletId, amount = r.Amount, status = r.Status, approved_at = r.ApprovedAt });
    }

    private static async Task<IResult> RejectWithdrawal(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var r = await db.WithdrawalRequests.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (r == null) return Results.NotFound();
        if (wid.HasValue)
        {
            var wallet = await db.Wallets.AsNoTracking().FirstOrDefaultAsync(w => w.Id == r.WalletId && w.WorkspaceId == wid.Value, ct);
            if (wallet == null) return Results.NotFound();
        }
        var body = await ctx.Request.ReadFromJsonAsync<RejectWithdrawalBody>(ct);
        r.Status = "rejected";
        r.RejectionReason = body?.reason ?? "";
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = r.Id, wallet_id = r.WalletId, amount = r.Amount, status = r.Status, rejection_reason = r.RejectionReason });
    }

    // -------------------------------------------------------------------------
    // Payment Methods
    // -------------------------------------------------------------------------

    private static async Task<IResult> ListPaymentMethods(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.PaymentMethods.AsNoTracking().Where(m => m.IsActive && (!wid.HasValue || m.WorkspaceId == wid.Value));
        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending(m => m.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new { id = m.Id, workspace_id = m.WorkspaceId, customer_id = m.CustomerId, gateway_id = m.GatewayId, method_type = m.MethodType, cardholder_name = m.CardholderName, card_brand = m.CardBrand, card_last4 = m.CardLast4, is_default = m.IsDefault, is_active = m.IsActive })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreatePaymentMethod(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var body = await ctx.Request.ReadFromJsonAsync<PaymentMethodCreateBody>(ct);
        if (body == null || body.customer_id <= 0 || body.gateway_id <= 0)
            return Results.BadRequest(new { detail = "customer_id and gateway_id are required." });
        var m = new PaymentMethod
        {
            WorkspaceId = wid,
            CustomerId = body.customer_id,
            GatewayId = body.gateway_id,
            MethodType = body.method_type ?? "card",
            GatewayToken = body.gateway_token,
            CardholderName = body.cardholder_name ?? "",
            CardBrand = body.card_brand,
            CardLast4 = body.card_last4,
            CardExpMonth = body.card_exp_month,
            CardExpYear = body.card_exp_year,
            DisplayInfo = body.display_info,
            IsDefault = body.is_default ?? false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.PaymentMethods.Add(m);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/finance/payment-methods/", new { id = m.Id, customer_id = m.CustomerId, gateway_id = m.GatewayId, method_type = m.MethodType, is_default = m.IsDefault });
    }

    private static async Task<IResult> GetPaymentMethod(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.PaymentMethods.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        return Results.Ok(new { id = m.Id, workspace_id = m.WorkspaceId, customer_id = m.CustomerId, gateway_id = m.GatewayId, method_type = m.MethodType, cardholder_name = m.CardholderName, card_brand = m.CardBrand, card_last4 = m.CardLast4, card_exp_month = m.CardExpMonth, card_exp_year = m.CardExpYear, display_info = m.DisplayInfo, is_default = m.IsDefault, is_active = m.IsActive });
    }

    private static async Task<IResult> PatchPaymentMethod(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PaymentMethodPatchBody>(ct);
        if (body != null)
        {
            if (body.method_type != null) m.MethodType = body.method_type;
            if (body.cardholder_name != null) m.CardholderName = body.cardholder_name;
            if (body.card_brand != null) m.CardBrand = body.card_brand;
            if (body.card_last4 != null) m.CardLast4 = body.card_last4;
            if (body.card_exp_month.HasValue) m.CardExpMonth = body.card_exp_month.Value;
            if (body.card_exp_year.HasValue) m.CardExpYear = body.card_exp_year.Value;
            if (body.display_info != null) m.DisplayInfo = body.display_info;
            if (body.is_default.HasValue) m.IsDefault = body.is_default.Value;
            if (body.is_active.HasValue) m.IsActive = body.is_active.Value;
            m.UpdatedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = m.Id, customer_id = m.CustomerId, gateway_id = m.GatewayId, method_type = m.MethodType, cardholder_name = m.CardholderName, card_brand = m.CardBrand, card_last4 = m.CardLast4, is_default = m.IsDefault, is_active = m.IsActive });
    }

    private static async Task<IResult> DeletePaymentMethod(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var m = await db.PaymentMethods.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (m == null) return Results.NotFound();
        m.IsActive = false;
        m.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // -------------------------------------------------------------------------
    // Request body records
    // -------------------------------------------------------------------------

    // Currencies
    private sealed record CurrencyCreateBody(string? code, string? name, string? symbol, int? decimal_places);
    private sealed record CurrencyPatchBody(string? code, string? name, string? symbol, int? decimal_places, bool? is_active);

    // Payment Gateways
    private sealed record PaymentGatewayCreateBody(string? name, string? gateway_type, bool? is_active);
    private sealed record PaymentGatewayPatchBody(string? name, string? gateway_type, bool? is_active);

    // Payments
    private sealed record PaymentCreateBody(int order_id, int gateway_id, int currency_id, int order, int gateway, string? currency, string? amount, string? status);
    private sealed record PaymentPatchBody(string? status);

    // Invoices
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
    private sealed record InvoicePatchBody(string? status, string? notes, DateTime? due_date);

    // Invoice Items
    private sealed class InvoiceItemBody
    {
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public decimal? unit_price { get; set; }
        public decimal? discount { get; set; }
        public decimal? tax { get; set; }
        public string? tax_type { get; set; }
        public int? financial_code_id { get; set; }
        public int? product_id { get; set; }
    }
    private sealed class UpdateInvoiceItemsBody
    {
        public List<InvoiceItemBody>? items { get; set; }
    }
    private sealed class InvoiceItemCreateBody
    {
        public int invoice_id { get; set; }
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public decimal? unit_price { get; set; }
        public decimal? discount { get; set; }
        public decimal? tax { get; set; }
        public string? tax_type { get; set; }
        public int? financial_code_id { get; set; }
        public int? product_id { get; set; }
    }

    // Brands
    private sealed record BrandCreateBody(string? name, string? logo, bool? is_default, string? tax_id, string? registration_number, string? invoice_note);
    private sealed record BrandPatchBody(string? name, string? logo, bool? is_default, string? tax_id, string? registration_number, string? invoice_note);

    // Financial Codes
    private sealed record FinancialCodeCreateBody(string? code, string? name, string? description, decimal? unit_price, string? unit, string? tax_type);
    private sealed record FinancialCodePatchBody(string? code, string? name, string? description, decimal? unit_price, string? unit, string? tax_type, bool? is_active);

    // Tax Rates
    private sealed record TaxRateCreateBody(string? name, decimal? rate, string? country, string? state);
    private sealed record TaxRatePatchBody(string? name, decimal? rate, string? country, string? state, bool? is_active);

    // Wallets
    private sealed record WalletWithdrawBody(decimal amount, string? payout_method, string? payout_details, string? notes);

    // Withdrawal Requests
    private sealed record RejectWithdrawalBody(string? reason);

    // Payment Methods
    private sealed class PaymentMethodCreateBody
    {
        public int customer_id { get; set; }
        public int gateway_id { get; set; }
        public string? method_type { get; set; }
        public string? gateway_token { get; set; }
        public string? cardholder_name { get; set; }
        public string? card_brand { get; set; }
        public string? card_last4 { get; set; }
        public int? card_exp_month { get; set; }
        public int? card_exp_year { get; set; }
        public string? display_info { get; set; }
        public bool? is_default { get; set; }
    }
    private sealed class PaymentMethodPatchBody
    {
        public string? method_type { get; set; }
        public string? cardholder_name { get; set; }
        public string? card_brand { get; set; }
        public string? card_last4 { get; set; }
        public int? card_exp_month { get; set; }
        public int? card_exp_year { get; set; }
        public string? display_info { get; set; }
        public bool? is_default { get; set; }
        public bool? is_active { get; set; }
    }
}
