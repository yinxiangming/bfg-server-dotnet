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
        var c = new Carrier { WorkspaceId = wid.Value, Name = body.name ?? "", Code = body.code ?? "", IsActive = body.is_active ?? true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
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
            BasePrice = decimal.TryParse(body.base_price, out var bp) ? bp : 0,
            PricePerKg = decimal.TryParse(body.price_per_kg, out var ppk) ? ppk : 0,
            IsActive = body.is_active ?? true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
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

    private sealed record WarehouseCreateBody(string? name, string? code, bool? is_active);
    private sealed record WarehousePatchBody(string? name);
    private sealed record CarrierCreateBody(string? name, string? code, bool? is_active);
    private sealed record FreightServiceCreateBody(int carrier, string? name, string? code, string? base_price, string? price_per_kg, bool? is_active);
    private sealed record DeliveryZoneCreateBody(string? name);
    private sealed record ShipmentCreateBody(int? order_id, int? carrier_id, string? tracking_number, string? status);
}
