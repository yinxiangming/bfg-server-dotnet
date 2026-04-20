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
        var root = app.MapGroup("/api/v1").WithTags("Delivery").RequireAuthorization();

        // Warehouses
        group.MapGet("/warehouses", ListWarehouses);
        group.MapPost("/warehouses/", CreateWarehouse);
        group.MapGet("/warehouses/{id:int}", GetWarehouse);
        group.MapPatch("/warehouses/{id:int}", PatchWarehouse);
        group.MapDelete("/warehouses/{id:int}", DeleteWarehouse);
        group.MapPost("/warehouses/{id:int}/set_default", SetWarehouseDefault);

        root.MapGet("/warehouses", ListWarehouses);
        root.MapPost("/warehouses/", CreateWarehouse);
        root.MapGet("/warehouses/{id:int}", GetWarehouse);
        root.MapPatch("/warehouses/{id:int}", PatchWarehouse);
        root.MapDelete("/warehouses/{id:int}", DeleteWarehouse);
        root.MapPost("/warehouses/{id:int}/set_default", SetWarehouseDefault);

        // Carriers
        group.MapGet("/carriers", ListCarriers);
        group.MapPost("/carriers/", CreateCarrier);
        group.MapGet("/carriers/{id:int}", GetCarrier);
        group.MapPatch("/carriers/{id:int}", PatchCarrier);
        group.MapDelete("/carriers/{id:int}", DeleteCarrier);
        group.MapGet("/carriers/plugins", ListCarrierPlugins);
        group.MapGet("/carriers/config_schema", GetCarrierConfigSchema);

        root.MapGet("/carriers", ListCarriers);
        root.MapPost("/carriers/", CreateCarrier);
        root.MapGet("/carriers/{id:int}", GetCarrier);
        root.MapPatch("/carriers/{id:int}", PatchCarrier);
        root.MapDelete("/carriers/{id:int}", DeleteCarrier);
        root.MapGet("/carriers/plugins", ListCarrierPlugins);
        root.MapGet("/carriers/config_schema", GetCarrierConfigSchema);

        // Freight Services
        group.MapGet("/freight-services", ListFreightServices);
        group.MapPost("/freight-services/", CreateFreightService);
        group.MapGet("/freight-services/{id:int}", GetFreightService);
        group.MapPatch("/freight-services/{id:int}", PatchFreightService);
        group.MapDelete("/freight-services/{id:int}", DeleteFreightService);
        group.MapGet("/freight-services/templates", ListFreightServiceTemplates);
        group.MapGet("/freight-services/for_country", GetFreightServicesForCountry);

        root.MapGet("/freight-services", ListFreightServices);
        root.MapPost("/freight-services/", CreateFreightService);
        root.MapGet("/freight-services/{id:int}", GetFreightService);
        root.MapPatch("/freight-services/{id:int}", PatchFreightService);
        root.MapDelete("/freight-services/{id:int}", DeleteFreightService);
        root.MapGet("/freight-services/templates", ListFreightServiceTemplates);
        root.MapGet("/freight-services/for_country", GetFreightServicesForCountry);

        // Delivery Zones
        group.MapGet("/delivery-zones", ListDeliveryZones);
        group.MapPost("/delivery-zones/", CreateDeliveryZone);
        group.MapPatch("/delivery-zones/{id:int}", PatchDeliveryZone);
        group.MapDelete("/delivery-zones/{id:int}", DeleteDeliveryZone);

        root.MapGet("/delivery-zones", ListDeliveryZones);
        root.MapPost("/delivery-zones/", CreateDeliveryZone);
        root.MapPatch("/delivery-zones/{id:int}", PatchDeliveryZone);
        root.MapDelete("/delivery-zones/{id:int}", DeleteDeliveryZone);

        // Shipments
        group.MapGet("/shipments", ListShipments);
        group.MapPost("/shipments/", CreateShipment);

        root.MapGet("/shipments", ListShipments);
        root.MapPost("/shipments/", CreateShipment);

        // Package Templates
        group.MapPost("/package-templates/", CreatePackageTemplate);
        group.MapGet("/package-templates/", ListPackageTemplates);
        group.MapGet("/package-templates/{id:int}/", GetPackageTemplate);
        group.MapPatch("/package-templates/{id:int}/", PatchPackageTemplate);

        root.MapPost("/package-templates/", CreatePackageTemplate);
        root.MapGet("/package-templates/", ListPackageTemplates);
        root.MapGet("/package-templates/{id:int}/", GetPackageTemplate);
        root.MapPatch("/package-templates/{id:int}/", PatchPackageTemplate);

        // Freight Statuses
        group.MapGet("/freight-statuses", ListFreightStatuses);
        group.MapPost("/freight-statuses/", CreateFreightStatus);
        group.MapGet("/freight-statuses/{id:int}", GetFreightStatus);
        group.MapPatch("/freight-statuses/{id:int}", PatchFreightStatus);
        group.MapDelete("/freight-statuses/{id:int}", DeleteFreightStatus);

        root.MapGet("/freight-statuses", ListFreightStatuses);
        root.MapPost("/freight-statuses/", CreateFreightStatus);
        root.MapGet("/freight-statuses/{id:int}", GetFreightStatus);
        root.MapPatch("/freight-statuses/{id:int}", PatchFreightStatus);
        root.MapDelete("/freight-statuses/{id:int}", DeleteFreightStatus);

        // Packaging Types
        group.MapGet("/packaging-types", ListPackagingTypes);
        group.MapPost("/packaging-types/", CreatePackagingType);
        group.MapGet("/packaging-types/{id:int}", GetPackagingType);
        group.MapPatch("/packaging-types/{id:int}", PatchPackagingType);
        group.MapDelete("/packaging-types/{id:int}", DeletePackagingType);

        root.MapGet("/packaging-types", ListPackagingTypes);
        root.MapPost("/packaging-types/", CreatePackagingType);
        root.MapGet("/packaging-types/{id:int}", GetPackagingType);
        root.MapPatch("/packaging-types/{id:int}", PatchPackagingType);
        root.MapDelete("/packaging-types/{id:int}", DeletePackagingType);

        // Packages
        group.MapGet("/packages", ListPackages);
        group.MapPost("/packages/", CreatePackage);
        group.MapGet("/packages/{id:int}", GetPackage);
        group.MapPatch("/packages/{id:int}", PatchPackage);

        root.MapGet("/packages", ListPackages);
        root.MapPost("/packages/", CreatePackage);
        root.MapGet("/packages/{id:int}", GetPackage);
        root.MapPatch("/packages/{id:int}", PatchPackage);

        // Consignments
        group.MapGet("/consignments", ListConsignments);
        group.MapPost("/consignments/", CreateConsignment);
        group.MapPatch("/consignments/{refKey}/", PatchConsignment);
        group.MapDelete("/consignments/by-id/{id:int}", DeleteConsignment);
        group.MapPost("/consignments/{consignmentNumber}/cancel", CancelConsignment);
        group.MapPost("/consignments/{consignmentNumber}/update_status", UpdateConsignmentStatus);
        group.MapPost("/consignments/{consignmentNumber}/add_tracking_event", AddConsignmentTrackingEvent);

        root.MapGet("/consignments", ListConsignments);
        root.MapPost("/consignments/", CreateConsignment);
        root.MapPatch("/consignments/{refKey}/", PatchConsignment);
        root.MapDelete("/consignments/by-id/{id:int}", DeleteConsignment);
        root.MapPost("/consignments/{consignmentNumber}/cancel", CancelConsignment);
        root.MapPost("/consignments/{consignmentNumber}/update_status", UpdateConsignmentStatus);
        root.MapPost("/consignments/{consignmentNumber}/add_tracking_event", AddConsignmentTrackingEvent);

        // Tracking Events
        group.MapGet("/tracking-events", ListTrackingEvents);
        group.MapPost("/tracking-events/", CreateTrackingEvent);
        group.MapGet("/tracking-events/{id:int}", GetTrackingEvent);

        root.MapGet("/tracking-events", ListTrackingEvents);
        root.MapPost("/tracking-events/", CreateTrackingEvent);
        root.MapGet("/tracking-events/{id:int}", GetTrackingEvent);
    }

    // ---------------------------------------------------------------------------
    // Warehouses
    // ---------------------------------------------------------------------------

    private static async Task<IResult> ListWarehouses(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Warehouses.AsNoTracking().Where(w => w.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(w => w.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(w => new { id = w.Id, workspace_id = w.WorkspaceId, name = w.Name, code = w.Code, is_active = w.IsActive, is_default = w.IsDefault, created_at = w.CreatedAt })
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
        return Results.Ok(new { id = w.Id, name = w.Name, code = w.Code, is_active = w.IsActive, is_default = w.IsDefault, workspace_id = w.WorkspaceId });
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

    private static async Task<IResult> SetWarehouseDefault(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var target = await db.Warehouses.FirstOrDefaultAsync(x => x.Id == id && x.WorkspaceId == wid.Value, ct);
        if (target == null) return Results.NotFound();
        var others = await db.Warehouses.Where(x => x.WorkspaceId == wid.Value && x.IsDefault && x.Id != id).ToListAsync(ct);
        foreach (var o in others)
        {
            o.IsDefault = false;
            o.UpdatedAt = DateTime.UtcNow;
        }
        target.IsDefault = true;
        target.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = target.Id, name = target.Name, is_default = target.IsDefault });
    }

    // ---------------------------------------------------------------------------
    // Carriers
    // ---------------------------------------------------------------------------

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
        return Results.Ok(new { id = c.Id, name = c.Name, code = c.Code, is_active = c.IsActive });
    }

    private static async Task<IResult> PatchCarrier(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Carriers.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CarrierPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) c.Name = body.name;
            if (body.config != null) c.Config = body.config;
            if (body.is_active.HasValue) c.IsActive = body.is_active.Value;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, name = c.Name, code = c.Code, is_active = c.IsActive });
    }

    private static async Task<IResult> DeleteCarrier(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Carriers.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        c.IsActive = false;
        c.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static IResult ListCarrierPlugins()
    {
        var plugins = new[]
        {
            new { name = "Custom", code = "custom" },
            new { name = "NZ Post", code = "nzpost" },
            new { name = "DHL", code = "dhl" },
            new { name = "FedEx", code = "fedex" },
            new { name = "UPS", code = "ups" }
        };
        return Results.Ok(plugins);
    }

    private static IResult GetCarrierConfigSchema()
    {
        return Results.Ok(new { });
    }

    // ---------------------------------------------------------------------------
    // Freight Services
    // ---------------------------------------------------------------------------

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
        return Results.Ok(new { id = f.Id, name = f.Name, code = f.Code, base_price = f.BasePrice, is_active = f.IsActive });
    }

    private static async Task<IResult> PatchFreightService(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FreightServices.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<FreightServicePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) f.Name = body.name;
            if (body.code != null) f.Code = body.code;
            if (body.base_price != null && decimal.TryParse(body.base_price, out var bp)) f.BasePrice = bp;
            if (body.is_active.HasValue) f.IsActive = body.is_active.Value;
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = f.Id, name = f.Name, code = f.Code, base_price = f.BasePrice, is_active = f.IsActive });
    }

    private static async Task<IResult> DeleteFreightService(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FreightServices.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        f.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static IResult ListFreightServiceTemplates()
    {
        var templates = new[]
        {
            new { name = "Standard", code = "standard", transport_type = "road" },
            new { name = "Express", code = "express", transport_type = "air" },
            new { name = "Economy", code = "economy", transport_type = "road" },
            new { name = "Overnight", code = "overnight", transport_type = "air" },
            new { name = "International", code = "international", transport_type = "air" }
        };
        return Results.Ok(templates);
    }

    private static async Task<IResult> GetFreightServicesForCountry(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var country = req.Query["country"].FirstOrDefault();
        var query = db.FreightServices.AsNoTracking()
            .Where(f => (!wid.HasValue || f.WorkspaceId == wid.Value) && f.IsActive);
        var list = await query.OrderBy(f => f.SortOrder).ThenBy(f => f.Name)
            .Select(f => new { id = f.Id, name = f.Name, code = f.Code, base_price = f.BasePrice, is_active = f.IsActive })
            .ToListAsync(ct);
        return Results.Ok(list);
    }

    // ---------------------------------------------------------------------------
    // Delivery Zones
    // ---------------------------------------------------------------------------

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

    private static async Task<IResult> PatchDeliveryZone(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var z = await db.DeliveryZones.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (z == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<DeliveryZonePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) z.Name = body.name;
            if (body.is_active.HasValue) z.IsActive = body.is_active.Value;
            z.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = z.Id, name = z.Name, is_active = z.IsActive });
    }

    private static async Task<IResult> DeleteDeliveryZone(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var z = await db.DeliveryZones.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (z == null) return Results.NotFound();
        db.DeliveryZones.Remove(z);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ---------------------------------------------------------------------------
    // Shipments
    // ---------------------------------------------------------------------------

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

    // ---------------------------------------------------------------------------
    // Package Templates
    // ---------------------------------------------------------------------------

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

    // ---------------------------------------------------------------------------
    // Freight Statuses
    // ---------------------------------------------------------------------------

    private static async Task<IResult> ListFreightStatuses(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.FreightStatuses.AsNoTracking().Where(f => f.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(f => f.SortOrder).ThenBy(f => f.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(f => new { id = f.Id, code = f.Code, name = f.Name, type = f.Type, state = f.State, color = f.Color, is_active = f.IsActive })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
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

    private static async Task<IResult> GetFreightStatus(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FreightStatuses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        return Results.Ok(new { id = f.Id, code = f.Code, name = f.Name, type = f.Type, state = f.State, color = f.Color, is_active = f.IsActive, sort_order = f.SortOrder });
    }

    private static async Task<IResult> PatchFreightStatus(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FreightStatuses.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<FreightStatusPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) f.Name = body.name;
            if (body.code != null) f.Code = body.code;
            if (body.type != null) f.Type = body.type;
            if (body.state != null) f.State = body.state;
            if (body.color != null) f.Color = body.color;
            if (body.description != null) f.Description = body.description;
            if (body.is_active.HasValue) f.IsActive = body.is_active.Value;
            if (body.is_public.HasValue) f.IsPublic = body.is_public.Value;
            if (body.send_message.HasValue) f.SendMessage = body.send_message.Value;
            if (body.order.HasValue) f.SortOrder = body.order.Value;
            f.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = f.Id, code = f.Code, name = f.Name, type = f.Type, state = f.State });
    }

    private static async Task<IResult> DeleteFreightStatus(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var f = await db.FreightStatuses.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (f == null) return Results.NotFound();
        f.IsActive = false;
        f.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ---------------------------------------------------------------------------
    // Packaging Types
    // ---------------------------------------------------------------------------

    private static async Task<IResult> ListPackagingTypes(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.PackagingTypes.AsNoTracking().Where(p => p.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var list = await query.OrderBy(p => p.Name).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new { id = p.Id, name = p.Name, code = p.Code, is_active = p.IsActive })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreatePackagingType(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<PackagingTypeCreateBody>(ct);
        if (body == null || string.IsNullOrWhiteSpace(body.name)) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var p = new PackagingType
        {
            WorkspaceId = wid.Value,
            Name = body.name,
            Code = body.code ?? "",
            IsActive = body.is_active ?? true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.PackagingTypes.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/packaging-types/", new { id = p.Id, name = p.Name, code = p.Code, is_active = p.IsActive });
    }

    private static async Task<IResult> GetPackagingType(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.PackagingTypes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { id = p.Id, name = p.Name, code = p.Code, is_active = p.IsActive });
    }

    private static async Task<IResult> PatchPackagingType(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.PackagingTypes.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PackagingTypePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) p.Name = body.name;
            if (body.code != null) p.Code = body.code;
            if (body.is_active.HasValue) p.IsActive = body.is_active.Value;
            p.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = p.Id, name = p.Name, code = p.Code, is_active = p.IsActive });
    }

    private static async Task<IResult> DeletePackagingType(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var p = await db.PackagingTypes.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (p == null) return Results.NotFound();
        db.PackagingTypes.Remove(p);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ---------------------------------------------------------------------------
    // Packages (DeliveryPackage)
    // ---------------------------------------------------------------------------

    private static async Task<IResult> ListPackages(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);
        var orderIds = wid.HasValue
            ? await db.Orders.AsNoTracking().Where(o => o.WorkspaceId == wid.Value).Select(o => o.Id).ToListAsync(ct)
            : null;
        var query = db.DeliveryPackages.AsNoTracking();
        if (orderIds != null) query = query.Where(p => p.OrderId.HasValue && orderIds.Contains(p.OrderId.Value));
        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending(p => p.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new { id = p.Id, consignment_id = p.ConsignmentId, weight = p.Weight, created_at = p.CreatedAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreatePackage(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<PackageCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var p = new DeliveryPackage
        {
            ConsignmentId = body.consignment_id,
            Weight = body.weight ?? 0,
            Length = body.length ?? 0,
            Width = body.width ?? 0,
            Height = body.height ?? 0,
            CreatedAt = now,
            PackageNumber = $"PKG-{Guid.NewGuid().ToString("N")[..8]}",
            State = "pending",
            Description = "",
            Notes = ""
        };
        db.DeliveryPackages.Add(p);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/packages/", new { id = p.Id, consignment_id = p.ConsignmentId, weight = p.Weight });
    }

    private static async Task<IResult> GetPackage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var p = await db.DeliveryPackages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p == null) return Results.NotFound();
        return Results.Ok(new { id = p.Id, consignment_id = p.ConsignmentId, weight = p.Weight, length = p.Length, width = p.Width, height = p.Height });
    }

    private static async Task<IResult> PatchPackage(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var p = await db.DeliveryPackages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<PackagePatchBody>(ct);
        if (body != null)
        {
            if (body.weight.HasValue) p.Weight = body.weight.Value;
            if (body.length.HasValue) p.Length = body.length.Value;
            if (body.width.HasValue) p.Width = body.width.Value;
            if (body.height.HasValue) p.Height = body.height.Value;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = p.Id, consignment_id = p.ConsignmentId, weight = p.Weight });
    }

    // ---------------------------------------------------------------------------
    // Consignments
    // ---------------------------------------------------------------------------

    private static async Task<IResult> ListConsignments(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Consignments.AsNoTracking().Where(c => c.WorkspaceId == wid.Value);
        var total = await query.CountAsync(ct);
        var raw = await query.OrderByDescending(c => c.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(c => new { c.Id, c.ConsignmentNumber, c.TrackingNumber, c.State, c.StatusId, c.ServiceId, c.CreatedAt })
            .ToListAsync(ct);
        var list = raw.Select(c => (object)new { id = c.Id, consignment_number = c.ConsignmentNumber, tracking_number = c.TrackingNumber, state = c.State, status = c.State, status_id = c.StatusId, service_id = c.ServiceId, created_at = c.CreatedAt }).ToList();
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
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

    private static async Task<IResult> DeleteConsignment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Consignments.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        db.Consignments.Remove(c);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelConsignment(BfgDbContext db, HttpContext ctx, string consignmentNumber, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Consignments.FirstOrDefaultAsync(x => x.ConsignmentNumber == consignmentNumber && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        c.State = "cancelled";
        c.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = c.Id, consignment_number = c.ConsignmentNumber, state = c.State });
    }

    private static async Task<IResult> UpdateConsignmentStatus(BfgDbContext db, HttpContext ctx, string consignmentNumber, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Consignments.FirstOrDefaultAsync(x => x.ConsignmentNumber == consignmentNumber && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<ConsignmentUpdateStatusBody>(ct);
        if (body?.state != null)
        {
            c.State = body.state;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, consignment_number = c.ConsignmentNumber, state = c.State });
    }

    private static async Task<IResult> AddConsignmentTrackingEvent(BfgDbContext db, HttpContext ctx, string consignmentNumber, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Consignments.AsNoTracking().FirstOrDefaultAsync(x => x.ConsignmentNumber == consignmentNumber && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<TrackingEventCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var ev = new TrackingEvent
        {
            WorkspaceId = wid.HasValue ? wid.Value : c.WorkspaceId,
            ConsignmentId = c.Id,
            PackageId = body.package_id,
            EventType = body.event_type ?? "",
            Description = body.description ?? "",
            Location = body.location ?? "",
            OccurredAt = body.occurred_at ?? now,
            CreatedAt = now
        };
        db.TrackingEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/tracking-events/", new { id = ev.Id, consignment_id = ev.ConsignmentId, event_type = ev.EventType, occurred_at = ev.OccurredAt });
    }

    // ---------------------------------------------------------------------------
    // Tracking Events
    // ---------------------------------------------------------------------------

    private static async Task<IResult> ListTrackingEvents(BfgDbContext db, HttpContext ctx, HttpRequest req, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.Ok(Pagination.Wrap(new List<object>(), 1, 20, 0));
        var (page, pageSize) = Pagination.FromRequest(req);

        var consignmentIdStr = req.Query["consignment_id"].FirstOrDefault();
        var packageIdStr = req.Query["package_id"].FirstOrDefault();

        var query = db.TrackingEvents.AsNoTracking().Where(e => e.WorkspaceId == wid.Value);
        if (int.TryParse(consignmentIdStr, out var consignmentId) && consignmentId > 0)
            query = query.Where(e => e.ConsignmentId == consignmentId);
        if (int.TryParse(packageIdStr, out var packageId) && packageId > 0)
            query = query.Where(e => e.PackageId == packageId);

        var total = await query.CountAsync(ct);
        var list = await query.OrderByDescending(e => e.OccurredAt).Skip((page - 1) * pageSize).Take(pageSize)
            .Select(e => new { id = e.Id, consignment_id = e.ConsignmentId, package_id = e.PackageId, event_type = e.EventType, description = e.Description, location = e.Location, occurred_at = e.OccurredAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateTrackingEvent(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<TrackingEventCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var now = DateTime.UtcNow;
        var ev = new TrackingEvent
        {
            WorkspaceId = wid.Value,
            ConsignmentId = body.consignment_id,
            PackageId = body.package_id,
            EventType = body.event_type ?? "",
            Description = body.description ?? "",
            Location = body.location ?? "",
            OccurredAt = body.occurred_at ?? now,
            CreatedAt = now
        };
        db.TrackingEvents.Add(ev);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/delivery/tracking-events/", new { id = ev.Id, consignment_id = ev.ConsignmentId, package_id = ev.PackageId, event_type = ev.EventType, occurred_at = ev.OccurredAt });
    }

    private static async Task<IResult> GetTrackingEvent(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var ev = await db.TrackingEvents.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (ev == null) return Results.NotFound();
        return Results.Ok(new { id = ev.Id, consignment_id = ev.ConsignmentId, package_id = ev.PackageId, event_type = ev.EventType, description = ev.Description, location = ev.Location, occurred_at = ev.OccurredAt, created_at = ev.CreatedAt });
    }

    // ---------------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------------

    private static decimal ParseDec(string? s) =>
        decimal.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;

    // ---------------------------------------------------------------------------
    // Request body records
    // ---------------------------------------------------------------------------

    private sealed record WarehouseCreateBody(string? name, string? code, bool? is_active);
    private sealed record WarehousePatchBody(string? name);

    private sealed record CarrierCreateBody(string? name, string? code, bool? is_active);
    private sealed record CarrierPatchBody(string? name, string? config, bool? is_active);

    private sealed record FreightServiceCreateBody(int carrier, string? name, string? code, string? base_price, string? price_per_kg, bool? is_active);
    private sealed record FreightServicePatchBody(string? name, string? code, string? base_price, bool? is_active);

    private sealed record DeliveryZoneCreateBody(string? name);
    private sealed record DeliveryZonePatchBody(string? name, bool? is_active);

    private sealed record ShipmentCreateBody(int? order_id, int? carrier_id, string? tracking_number, string? status);

    private sealed record PackageTemplateCreateBody(string? code, string? name, string? description, string? length, string? width, string? height, string? tare_weight, string? max_weight, int? order, bool? is_active);
    private sealed record PackageTemplatePatchBody(string? name, string? description);

    private sealed record FreightStatusCreateBody(string? code, string? name, string? type, string? state, string? description, string? color, int? order, bool? is_active, bool? is_public, bool? send_message);
    private sealed record FreightStatusPatchBody(string? code, string? name, string? type, string? state, string? description, string? color, int? order, bool? is_active, bool? is_public, bool? send_message);

    private sealed record PackagingTypeCreateBody(string? name, string? code, bool? is_active);
    private sealed record PackagingTypePatchBody(string? name, string? code, bool? is_active);

    private sealed record PackageCreateBody(int? consignment_id, string? tracking_number, decimal? weight, decimal? length, decimal? width, decimal? height);
    private sealed record PackagePatchBody(string? tracking_number, decimal? weight, decimal? length, decimal? width, decimal? height);

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
    private sealed record ConsignmentUpdateStatusBody(string? state);

    private sealed record TrackingEventCreateBody(int? consignment_id, int? package_id, string? event_type, string? description, string? location, DateTime? occurred_at);
}
