using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Core;
using Bfg.Core.Delivery;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class DeliveryEndpoints
{
    public static void MapDeliveryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/delivery").WithTags("Delivery").RequireAuthorization();

        group.MapGet("/warehouses", ListWarehouses);
        group.MapPost("/warehouses/", CreateWarehouse);
        group.MapGet("/warehouses/{id:int}", GetWarehouse);
        group.MapPatch("/warehouses/{id:int}", PatchWarehouse);
        group.MapDelete("/warehouses/{id:int}", DeleteWarehouse);

        group.MapGet("/carriers", ListCarriers);
        group.MapPost("/carriers/", CreateCarrier);
        group.MapGet("/carriers/{id:int}", GetCarrier);

        group.MapGet("/freight-services", ListFreightServices);
        group.MapPost("/freight-services/", CreateFreightService);
        group.MapGet("/freight-services/{id:int}", GetFreightService);

        group.MapGet("/delivery-zones", ListDeliveryZones);
        group.MapPost("/delivery-zones/", CreateDeliveryZone);

        group.MapGet("/shipments", ListShipments);
        group.MapPost("/shipments/", CreateShipment);

        group.MapPost("/package-templates/", CreatePackageTemplate);
        group.MapGet("/package-templates/", ListPackageTemplates);
        group.MapGet("/package-templates/{id:int}/", GetPackageTemplate);
        group.MapPatch("/package-templates/{id:int}/", PatchPackageTemplate);

        group.MapPost("/freight-statuses/", CreateFreightStatus);

        group.MapPost("/consignments/", CreateConsignment);
        group.MapPatch("/consignments/{refKey}/", PatchConsignment);
    }

    private static async Task<IResult> ListWarehouses(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Warehouses.AsNoTracking().Where(w => w.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(w => w.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(w => new { id = w.Id, workspace_id = w.WorkspaceId, name = w.Name, code = w.Code, is_active = w.IsActive, created_at = w.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateWarehouse(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<WarehouseCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var w = new Warehouse
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Code = body.code ?? "",
            Phone = "",
            Email = "",
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Warehouses.Add(w);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/warehouses/", new { id = w.Id, name = w.Name, code = w.Code, is_active = w.IsActive, workspace_id = w.WorkspaceId });
    }

    private static async Task<IResult> GetWarehouse(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var w = await db.Warehouses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid!.Value, ct);
        if (w == null) return Results.NotFound();
        return Results.Ok(new { id = w.Id, name = w.Name, code = w.Code, is_active = w.IsActive });
    }

    private static async Task<IResult> PatchWarehouse(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var w = await db.Warehouses.FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid!.Value, ct);
        if (w == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<WarehousePatchBody>(ct);
        if (body?.name != null) w.Name = body.name;
        w.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = w.Id, name = w.Name, code = w.Code, is_active = w.IsActive });
    }

    private static async Task<IResult> DeleteWarehouse(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var w = await db.Warehouses.FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid!.Value, ct);
        if (w == null) return Results.NotFound();
        db.Warehouses.Remove(w);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ListCarriers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Carriers.AsNoTracking().Where(c => !wid.HasValue || c.WorkspaceId == wid.Value);
        var list = await query.OrderBy(c => c.Name).Select(c => new { id = c.Id, name = c.Name, code = c.Code, is_active = c.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateCarrier(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CarrierCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var c = new Carrier
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Code = body.code ?? "",
            CarrierType = "standard",
            Config = "{}",
            TestConfig = "{}",
            IsTestMode = false,
            TrackingUrlTemplate = "",
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Carriers.Add(c);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/carriers/", new { id = c.Id, name = c.Name, code = c.Code });
    }

    private static async Task<IResult> GetCarrier(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Carriers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        return Results.Ok(new { id = c.Id, name = c.Name, code = c.Code });
    }

    private static async Task<IResult> ListFreightServices(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.FreightServices.AsNoTracking().Where(f => !wid.HasValue || f.WorkspaceId == wid.Value);
        var list = await query.OrderBy(f => f.Name).Select(f => new { id = f.Id, carrier_id = f.CarrierId, name = f.Name, code = f.Code, base_price = f.BasePrice, is_active = f.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateFreightService(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<FreightServiceCreateBody>(ct);
        if (body == null || body.carrier <= 0) return Results.BadRequest();
        var f = new FreightService
        {
            WorkspaceId = wid.Value,
            CarrierId = body.carrier,
            Name = body.name ?? "",
            Code = body.code ?? "",
            Description = "",
            BasePrice = decimal.TryParse(body.base_price, out var bp) ? bp : 0,
            PricePerKg = decimal.TryParse(body.price_per_kg, out var ppk) ? ppk : 0,
            EstimatedDaysMin = 1,
            EstimatedDaysMax = 7,
            MinWeight = 0,
            MaxWeight = null,
            Config = "{}",
            TransportType = "",
            IsActive = body.is_active ?? true,
            SortOrder = 100
        };
        db.FreightServices.Add(f);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/freight-services/", new { id = f.Id, name = f.Name, code = f.Code, base_price = f.BasePrice });
    }

    private static async Task<IResult> GetFreightService(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FreightServices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        return Results.Ok(new { id = f.Id, name = f.Name, code = f.Code, base_price = f.BasePrice });
    }

    private static async Task<IResult> ListDeliveryZones(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.DeliveryZones.AsNoTracking().Where(z => !wid.HasValue || z.WorkspaceId == wid.Value);
        var list = await query.Select(z => new { id = z.Id, name = z.Name }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateDeliveryZone(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<DeliveryZoneCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var z = new DeliveryZone { WorkspaceId = wid.Value, Name = body.name ?? "", IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.DeliveryZones.Add(z);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/delivery-zones/", new { id = z.Id, name = z.Name });
    }

    private static async Task<IResult> ListShipments(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Shipments.AsNoTracking().Where(s => !wid.HasValue || s.WorkspaceId == wid.Value);
        var list = await query.Select(s => new { id = s.Id, order_id = s.OrderId, tracking_number = s.TrackingNumber, status = s.Status }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateShipment(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<ShipmentCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var s = new Shipment { WorkspaceId = wid.Value, OrderId = body.order_id ?? 0, CarrierId = body.carrier_id, TrackingNumber = body.tracking_number, Status = body.status ?? "pending", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Shipments.Add(s);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/shipments/", new { id = s.Id, order_id = s.OrderId, status = s.Status });
    }

    private static async Task<IResult> CreatePackageTemplate(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<PackageTemplateCreateBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.code)) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var len = ParseDec(body.length);
        var w = ParseDec(body.width);
        var h = ParseDec(body.height);
        var t = new PackageTemplate
        {
            WorkspaceId = wid.Value,
            Code = body.code,
            Name = body.name ?? "",
            Description = body.description ?? "",
            Length = len,
            Width = w,
            Height = h,
            TareWeight = ParseDec(body.tare_weight),
            MaxWeight = string.IsNullOrEmpty(body.max_weight) ? null : ParseDec(body.max_weight),
            SortOrder = body.order ?? 0,
            IsActive = body.is_active ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.PackageTemplates.Add(t);
        await db.SaveChangesAsync(ct);
        var vol = len * w * h;
        return Results.Created("/api/v1/delivery/package-templates/", new
        {
            id = t.Id,
            code = t.Code,
            name = t.Name,
            description = t.Description,
            length = t.Length.ToString("F2"),
            width = t.Width.ToString("F2"),
            height = t.Height.ToString("F2"),
            volume_cm3 = vol > 0 ? vol.ToString("F2") : (string?)null
        });
    }

    private static async Task<IResult> ListPackageTemplates(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Array.Empty<object>());
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.PackageTemplates.AsNoTracking().Where(t => t.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(t => t.SortOrder).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(t => new { id = t.Id, code = t.Code, name = t.Name, length = t.Length, width = t.Width, height = t.Height })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> GetPackageTemplate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.PackageTemplates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, code = t.Code, name = t.Name, description = t.Description, length = t.Length.ToString("F2") });
    }

    private static async Task<IResult> PatchPackageTemplate(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.PackageTemplates.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PackageTemplatePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) t.Name = body.name;
            if (body.description != null) t.Description = body.description;
            t.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = t.Id, name = t.Name, description = t.Description });
    }

    private static async Task<IResult> CreateFreightStatus(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<FreightStatusCreateBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.code)) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var f = new FreightStatus
        {
            WorkspaceId = wid.Value,
            Code = body.code,
            Name = body.name ?? "",
            Type = body.type ?? "",
            State = string.IsNullOrEmpty(body.state) ? "PENDING" : body.state!,
            Description = body.description,
            Color = string.IsNullOrEmpty(body.color) ? "#000000" : body.color!,
            SortOrder = body.order ?? 0,
            IsActive = body.is_active ?? true,
            IsPublic = body.is_public ?? true,
            SendMessage = body.send_message ?? false,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.FreightStatuses.Add(f);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/freight-statuses/", new { id = f.Id, code = f.Code, name = f.Name, type = f.Type, state = f.State });
    }

    private static async Task<IResult> CreateConsignment(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<ConsignmentCreateBody>(ct);
        if (body?.order_ids == null || body.order_ids.Count == 0 || body.status_id is null or <= 0)
            return Results.BadRequest(new { detail = "order_ids and status_id are required." });
        var st = await db.FreightStatuses.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == body.status_id && s.WorkspaceId == wid.Value, ct);
        if (st == null) return Results.BadRequest(new { detail = "Invalid status_id." });

        var orderIds = body.order_ids.Where(id => id > 0).Distinct().ToList();
        var orders = await db.Orders.AsNoTracking()
            .Where(o => orderIds.Contains(o.Id) && o.WorkspaceId == wid.Value)
            .ToListAsync(ct);
        if (orders.Count == 0) return Results.BadRequest(new { detail = "No valid orders." });

        var serviceId = body.service_id;
        if (serviceId is null or <= 0)
            serviceId = await db.FreightServices.AsNoTracking().Where(s => s.WorkspaceId == wid.Value).Select(s => s.Id).FirstOrDefaultAsync(ct);
        if (serviceId is null or <= 0)
            return Results.BadRequest(new { detail = "service_id is required." });

        var recipientId = body.recipient_address_id;
        if (recipientId is null or <= 0)
            recipientId = orders[0].ShippingAddressId;
        var senderId = body.sender_address_id ?? recipientId;
        if (recipientId is null or <= 0 || senderId is null or <= 0)
            return Results.BadRequest(new { detail = "sender and recipient addresses are required." });

        if (!await db.Addresses.AsNoTracking().AnyAsync(a => a.Id == recipientId && a.WorkspaceId == wid.Value, ct))
            return Results.BadRequest(new { detail = "Invalid recipient address." });
        if (!await db.Addresses.AsNoTracking().AnyAsync(a => a.Id == senderId && a.WorkspaceId == wid.Value, ct))
            return Results.BadRequest(new { detail = "Invalid sender address." });

        var now = DateTime.UtcNow;
        var num = "CON-" + Guid.NewGuid().ToString("N")[..12];
        var state = string.IsNullOrEmpty(body.state) ? (string.IsNullOrEmpty(st.State) ? "PENDING" : st.State) : body.state!;
        var c = new Consignment
        {
            WorkspaceId = wid.Value,
            ConsignmentNumber = num,
            TrackingNumber = "",
            State = state,
            Notes = "",
            CreatedAt = now,
            UpdatedAt = now,
            RecipientAddressId = recipientId.Value,
            SenderAddressId = senderId.Value,
            ServiceId = serviceId.Value,
            StatusId = body.status_id.Value
        };
        db.Consignments.Add(c);
        await db.SaveChangesAsync(ct);
        foreach (var oid in orderIds)
            db.ConsignmentOrders.Add(new ConsignmentOrder { ConsignmentId = c.Id, OrderId = oid });
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/consignments/", new { id = c.Id, consignment_number = c.ConsignmentNumber, state = c.State, status = c.State });
    }

    private static async Task<IResult> PatchConsignment(BfgDbContext db, HttpContext ctx, string refKey, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        Consignment? c = null;
        if (int.TryParse(refKey, out var idNum))
            c = await db.Consignments.FirstOrDefaultAsync(x => x.Id == idNum && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null)
            c = await db.Consignments.FirstOrDefaultAsync(x => x.ConsignmentNumber == refKey && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<ConsignmentPatchBody>(ct);
        if (body?.tracking_number != null)
        {
            c.TrackingNumber = body.tracking_number;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, consignment_number = c.ConsignmentNumber, tracking_number = c.TrackingNumber, state = c.State });
    }

    private static decimal ParseDec(string? s) =>
        decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;

    private sealed record WarehouseCreateBody(string? name, string? code, bool? is_active);
    private sealed record WarehousePatchBody(string? name);
    private sealed record CarrierCreateBody(string? name, string? code, bool? is_active);
    private sealed record FreightServiceCreateBody(int carrier, string? name, string? code, string? base_price, string? price_per_kg, bool? is_active);
    private sealed record DeliveryZoneCreateBody(string? name);
    private sealed record ShipmentCreateBody(int? order_id, int? carrier_id, string? tracking_number, string? status);

    private sealed record PackageTemplateCreateBody(string? code, string? name, string? description, string? length, string? width, string? height, string? tare_weight, string? max_weight, int? order, bool? is_active);
    private sealed record PackageTemplatePatchBody(string? name, string? description);
    private sealed record FreightStatusCreateBody(string? code, string? name, string? type, string? state, string? description, string? color, int? order, bool? is_active, bool? is_public, bool? send_message);

    private sealed class ConsignmentCreateBody
    {
        public List<int>? order_ids { get; set; }
        public int? status_id { get; set; }
        public int? service_id { get; set; }
        public int? sender_address_id { get; set; }
        public int? recipient_address_id { get; set; }
        public string? state { get; set; }
    }

    private sealed record ConsignmentPatchBody(string? tracking_number);
}
