using System.Security.Claims;
using System.Security.Cryptography;
using Bfg.Api.Infrastructure;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using Bfg.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Api.Endpoints;

public static class CommonEndpoints
{
    public static void MapCommonEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Common").RequireAuthorization();

        // Workspaces
        group.MapGet("/workspaces", ListWorkspaces);
        group.MapPost("/workspaces/", CreateWorkspace);
        group.MapGet("/workspaces/{id:int}", GetWorkspace);
        group.MapPatch("/workspaces/{id:int}", PatchWorkspace);
        group.MapDelete("/workspaces/{id:int}", DeleteWorkspace);

        // Customers
        group.MapGet("/customers", ListCustomers);
        group.MapPost("/customers/", CreateCustomer);
        group.MapGet("/customers/{id:int}", GetCustomer);
        group.MapPatch("/customers/{id:int}", PatchCustomer);
        group.MapDelete("/customers/{id:int}", DeleteCustomer);

        // Addresses
        group.MapGet("/addresses", ListAddresses);
        group.MapPost("/addresses/", CreateAddress);
        group.MapGet("/addresses/{id:int}", GetAddress);
        group.MapPatch("/addresses/{id:int}", PatchAddress);
        group.MapDelete("/addresses/{id:int}", DeleteAddress);

        // Settings
        group.MapGet("/settings", ListSettings);
        group.MapPatch("/settings/{id:int}", PatchSettings);

        // Email Configs
        group.MapGet("/email-configs", ListEmailConfigs);
        group.MapPost("/email-configs/", CreateEmailConfig);
        group.MapGet("/email-configs/{id:int}", GetEmailConfig);
        group.MapPatch("/email-configs/{id:int}", PatchEmailConfig);
        group.MapDelete("/email-configs/{id:int}", DeleteEmailConfig);
        group.MapPost("/email-configs/{id:int}/set_default", SetEmailConfigDefault);

        // Users
        group.MapGet("/users", ListUsers);
        group.MapPost("/users/", CreateUser);
        group.MapGet("/users/{id:int}", GetUser);
        group.MapPatch("/users/{id:int}", PatchUser);
        group.MapDelete("/users/{id:int}", DeleteUser);

        // Customer Tags
        group.MapPost("/customer-tags/", CreateCustomerTag);
        group.MapGet("/customer-tags", ListCustomerTags);
        group.MapGet("/customer-tags/{id:int}", GetCustomerTag);
        group.MapPatch("/customer-tags/{id:int}", PatchCustomerTag);
        group.MapDelete("/customer-tags/{id:int}", DeleteCustomerTag);
        group.MapPost("/customer-tags/{id:int}/tag_customers", TagCustomers);
        group.MapPost("/customer-tags/{id:int}/untag_customers", UntagCustomers);

        // Customer Segments
        group.MapPost("/customer-segments/", CreateCustomerSegment);
        group.MapGet("/customer-segments", ListCustomerSegments);
        group.MapGet("/customer-segments/{id:int}", GetCustomerSegment);
        group.MapPatch("/customer-segments/{id:int}", PatchCustomerSegment);
        group.MapDelete("/customer-segments/{id:int}", DeleteCustomerSegment);

        // Staff Roles
        group.MapGet("/staff-roles", ListStaffRoles);
        group.MapPost("/staff-roles/", CreateStaffRole);
        group.MapGet("/staff-roles/{id:int}", GetStaffRole);
        group.MapPatch("/staff-roles/{id:int}", PatchStaffRole);
        group.MapDelete("/staff-roles/{id:int}", DeleteStaffRole);

        // API Keys
        group.MapGet("/api-keys", ListApiKeys);
        group.MapPost("/api-keys/", CreateApiKey);
        group.MapGet("/api-keys/{id:int}", GetApiKey);
        group.MapPatch("/api-keys/{id:int}", PatchApiKey);
        group.MapDelete("/api-keys/{id:int}", DeleteApiKey);
        group.MapPost("/api-keys/{id:int}/regenerate", RegenerateApiKey);

        // Staff / Misc (existing)
        group.MapGet("/staff-members", ListStaffMembers);
        // /me is handled by MeEndpoints (/api/v1/me/)
        group.MapGet("/options/", Options);
        group.MapGet("/countries/", Countries);
    }

    // ── Workspaces ────────────────────────────────────────────────────────────

    private static async Task<IResult> ListWorkspaces(BfgDbContext db, HttpRequest req, CancellationToken ct)
    {
        var (page, pageSize) = Pagination.FromRequest(req);
        var query = db.Workspaces.AsNoTracking().Where(w => w.IsActive).OrderBy(w => w.Name);
        var total = await query.CountAsync(ct);
        var list = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(w => new { id = w.Id, name = w.Name, uuid = w.Uuid, slug = w.Slug, email = w.Email, phone = w.Phone, is_active = w.IsActive, settings = w.Settings, created_at = w.CreatedAt, updated_at = w.UpdatedAt })
            .ToListAsync(ct);
        return Results.Ok(Pagination.Wrap(list, page, pageSize, total));
    }

    private static async Task<IResult> CreateWorkspace(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<WorkspaceCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var slug = body.slug ?? Slugify(body.name ?? "workspace");
        if (await db.Workspaces.AnyAsync(w => w.Slug == slug, ct))
            return Results.Conflict(new { detail = "Workspace with this slug already exists." });
        var w = new Workspace
        {
            Name = body.name ?? "",
            Uuid = Guid.NewGuid().ToString("N"),
            Slug = slug,
            Email = body.email ?? "",
            IsActive = true,
            Settings = "{}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Workspaces.Add(w);
        await db.SaveChangesAsync(ct);
        var settings = new Settings { WorkspaceId = w.Id, SiteName = w.Name, DefaultLanguage = "en", DefaultCurrency = "NZD", UpdatedAt = DateTime.UtcNow };
        db.Settings.Add(settings);
        var adminRole = new StaffRole { WorkspaceId = w.Id, Name = "Admin", Code = "admin", Description = "Administrator", IsSystem = true, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.StaffRoles.Add(adminRole);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/workspaces/", new { id = w.Id, name = w.Name, slug = w.Slug, is_active = w.IsActive, created_at = w.CreatedAt, updated_at = w.UpdatedAt });
    }

    private static async Task<IResult> GetWorkspace(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var w = await db.Workspaces.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (w == null) return Results.NotFound();
        return Results.Ok(new { id = w.Id, name = w.Name, uuid = w.Uuid, slug = w.Slug, email = w.Email, phone = w.Phone, is_active = w.IsActive, settings = w.Settings, created_at = w.CreatedAt, updated_at = w.UpdatedAt });
    }

    private static async Task<IResult> PatchWorkspace(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var w = await db.Workspaces.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (w == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<WorkspacePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) w.Name = body.name;
            if (body.email != null) w.Email = body.email;
            if (body.phone != null) w.Phone = body.phone;
            w.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = w.Id, name = w.Name, email = w.Email, phone = w.Phone, updated_at = w.UpdatedAt });
    }

    private static async Task<IResult> DeleteWorkspace(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var w = await db.Workspaces.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (w == null) return Results.NotFound();
        w.IsActive = false;
        w.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Customers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> ListCustomers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Customers.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(c => c.CreatedAt).Select(c => new { id = c.Id, customer_number = c.CustomerNumber, is_active = c.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateCustomer(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest(new { detail = "No workspace. Send X-Workspace-Id header." });
        var body = await ctx.Request.ReadFromJsonAsync<CustomerCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var userId = body.user > 0 ? body.user : body.user_id;
        if (userId <= 0) return Results.BadRequest();
        var existing = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.WorkspaceId == wid.Value && c.UserId == userId, ct);
        if (existing != null)
        {
            var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == existing.UserId, ct);
            return Results.Ok(new { id = existing.Id, workspace = existing.WorkspaceId, user = new { id = u?.Id, username = u?.Username, email = u?.Email }, customer_number = existing.CustomerNumber, company_name = existing.CompanyName, tax_number = existing.TaxNumber, is_active = existing.IsActive, created_at = existing.CreatedAt });
        }
        var linkedUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (linkedUser == null)
            return Results.BadRequest(new { user_id = new[] { "User does not exist." } });
        var customerNumber = await CustomerNumberService.GetNextForWorkspaceAsync(wid.Value, ws => GetMaxCustomerSequenceAsync(db, ws, ct));
        var c = new Customer
        {
            WorkspaceId = wid.Value,
            UserId = userId,
            CompanyName = body.company_name ?? "",
            TaxNumber = body.tax_number ?? "",
            CustomerNumber = customerNumber,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Customers.Add(c);
        await db.SaveChangesAsync(ct);
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == c.UserId, ct);
        return Results.Created("/api/v1/customers/", new { id = c.Id, workspace = c.WorkspaceId, user = user != null ? new { id = user.Id, username = user.Username, email = user.Email } : (object?)null, customer_number = c.CustomerNumber, company_name = c.CompanyName, tax_number = c.TaxNumber, is_active = c.IsActive, created_at = c.CreatedAt });
    }

    private static async Task<IResult> GetCustomer(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == c.UserId, ct);
        return Results.Ok(new { id = c.Id, workspace = c.WorkspaceId, user = user != null ? new { id = user.Id, username = user.Username, email = user.Email } : (object?)null, customer_number = c.CustomerNumber, company_name = c.CompanyName, tax_number = c.TaxNumber, is_active = c.IsActive, created_at = c.CreatedAt });
    }

    private static async Task<IResult> PatchCustomer(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerPatchBody>(ct);
        if (body != null)
        {
            if (body.company_name != null) c.CompanyName = body.company_name;
            if (body.tax_number != null) c.TaxNumber = body.tax_number;
            if (body.is_active.HasValue) c.IsActive = body.is_active.Value;
            c.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = c.Id, company_name = c.CompanyName, tax_number = c.TaxNumber, is_active = c.IsActive });
    }

    private static async Task<IResult> DeleteCustomer(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var c = await db.Customers.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (c == null) return Results.NotFound();
        c.IsActive = false;
        c.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Addresses ─────────────────────────────────────────────────────────────

    private static async Task<IResult> ListAddresses(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Addresses.AsNoTracking();
        if (wid.HasValue) query = query.Where(a => a.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(a => a.CreatedAt).Select(a => new { id = a.Id, full_name = a.FullName, city = a.City, country = a.Country }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateAddress(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<AddressCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var a = new Address
        {
            WorkspaceId = wid.Value,
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
        return Results.Created("/api/v1/addresses/", new { id = a.Id, workspace = a.WorkspaceId, full_name = a.FullName, phone = a.Phone, email = a.Email, address_line1 = a.AddressLine1, address_line2 = a.AddressLine2, city = a.City, state = a.State, postal_code = a.PostalCode, country = a.Country, is_default = a.IsDefault, created_at = a.CreatedAt, updated_at = a.UpdatedAt });
    }

    private static async Task<IResult> GetAddress(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (a == null) return Results.NotFound();
        return Results.Ok(new { id = a.Id, full_name = a.FullName, city = a.City, country = a.Country });
    }

    private static async Task<IResult> PatchAddress(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (a == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<AddressPatchBody>(ct);
        if (body != null)
        {
            if (body.full_name != null) a.FullName = body.full_name;
            if (body.phone != null) a.Phone = body.phone;
            if (body.email != null) a.Email = body.email;
            if (body.address_line1 != null) a.AddressLine1 = body.address_line1;
            if (body.address_line2 != null) a.AddressLine2 = body.address_line2;
            if (body.city != null) a.City = body.city;
            if (body.state != null) a.State = body.state;
            if (body.postal_code != null) a.PostalCode = body.postal_code;
            if (body.country != null) a.Country = body.country;
            if (body.is_default.HasValue) a.IsDefault = body.is_default.Value;
            a.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = a.Id, full_name = a.FullName, city = a.City, country = a.Country, is_default = a.IsDefault });
    }

    private static async Task<IResult> DeleteAddress(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var a = await db.Addresses.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (a == null) return Results.NotFound();
        db.Addresses.Remove(a);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    private static async Task<IResult> ListSettings(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.Settings.AsNoTracking().Where(s => wid == null || s.WorkspaceId == wid.Value).Select(s => new { id = s.Id, workspace_id = s.WorkspaceId, site_name = s.SiteName }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> PatchSettings(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.Settings.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<SettingsPatchBody>(ct);
        if (body != null)
        {
            if (body.site_name != null) s.SiteName = body.site_name;
            if (body.default_language != null) s.DefaultLanguage = body.default_language;
            if (body.default_currency != null) s.DefaultCurrency = body.default_currency;
            if (body.contact_email != null) s.ContactEmail = body.contact_email;
            s.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = s.Id, site_name = s.SiteName, default_language = s.DefaultLanguage, default_currency = s.DefaultCurrency, contact_email = s.ContactEmail });
    }

    // ── Email Configs ─────────────────────────────────────────────────────────

    private static async Task<IResult> ListEmailConfigs(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.EmailConfigs.AsNoTracking();
        if (wid.HasValue) query = query.Where(e => e.WorkspaceId == wid.Value);
        var list = await query.Select(e => new { id = e.Id, name = e.Name, backend_type = e.BackendType, is_default = e.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateEmailConfig(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<EmailConfigCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var configJson = System.Text.Json.JsonSerializer.Serialize(new { host = body.host ?? "", port = body.port ?? 587, username = body.username ?? "", use_tls = body.use_tls ?? true, from_email = body.from_email ?? "" });
        var e = new EmailConfig
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            BackendType = body.backend_type ?? "smtp",
            Config = configJson,
            IsDefault = body.is_default ?? false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.EmailConfigs.Add(e);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/email-configs/", new { id = e.Id, name = e.Name, backend_type = e.BackendType, is_default = e.IsDefault });
    }

    private static async Task<IResult> GetEmailConfig(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var e = await db.EmailConfigs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (e == null) return Results.NotFound();
        return Results.Ok(new { id = e.Id, name = e.Name, backend_type = e.BackendType, config = e.Config, is_default = e.IsDefault, is_active = e.IsActive });
    }

    private static async Task<IResult> PatchEmailConfig(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var e = await db.EmailConfigs.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (e == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<EmailConfigPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) e.Name = body.name;
            if (body.is_active.HasValue) e.IsActive = body.is_active.Value;
            e.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = e.Id, name = e.Name, backend_type = e.BackendType, config = e.Config, is_default = e.IsDefault, is_active = e.IsActive });
    }

    private static async Task<IResult> DeleteEmailConfig(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var e = await db.EmailConfigs.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (e == null) return Results.NotFound();
        e.IsActive = false;
        e.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> SetEmailConfigDefault(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var e = await db.EmailConfigs.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (e == null) return Results.NotFound();
        var others = await db.EmailConfigs.Where(x => x.WorkspaceId == e.WorkspaceId && x.IsDefault && x.Id != id).ToListAsync(ct);
        foreach (var o in others) { o.IsDefault = false; o.UpdatedAt = DateTime.UtcNow; }
        e.IsDefault = true;
        e.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = e.Id, is_default = e.IsDefault });
    }

    // ── Users ─────────────────────────────────────────────────────────────────

    private static async Task<IResult> ListUsers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Users.AsNoTracking();
        if (wid.HasValue) query = query.Where(u => u.DefaultWorkspaceId == wid.Value);
        var list = await query.Select(u => new { id = u.Id, username = u.Username, email = u.Email, is_active = u.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateUser(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var body = await ctx.Request.ReadFromJsonAsync<UserCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        if (string.IsNullOrEmpty(body.username) || string.IsNullOrEmpty(body.email))
            return Results.BadRequest(new { detail = "username and email are required." });
        if (await db.Users.AnyAsync(u => u.Username == body.username, ct))
            return Results.Conflict(new { detail = "Username already exists." });
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var u = new User
        {
            Username = body.username,
            Email = body.email,
            FirstName = body.first_name ?? "",
            LastName = body.last_name ?? "",
            Phone = body.phone ?? "",
            Password = string.IsNullOrEmpty(body.password) ? "" : BCrypt.Net.BCrypt.HashPassword(body.password),
            IsStaff = body.is_staff ?? false,
            IsActive = true,
            DefaultWorkspaceId = wid,
            DateJoined = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.Users.Add(u);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/users/", new { id = u.Id, username = u.Username, email = u.Email, is_active = u.IsActive });
    }

    private static async Task<IResult> GetUser(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var u = await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u == null) return Results.NotFound();
        return Results.Ok(new { id = u.Id, username = u.Username, email = u.Email, first_name = u.FirstName, last_name = u.LastName, phone = u.Phone, is_staff = u.IsStaff, is_active = u.IsActive, date_joined = u.DateJoined });
    }

    private static async Task<IResult> PatchUser(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<UserPatchBody>(ct);
        if (body != null)
        {
            if (body.first_name != null) u.FirstName = body.first_name;
            if (body.last_name != null) u.LastName = body.last_name;
            if (body.phone != null) u.Phone = body.phone;
            if (body.email != null) u.Email = body.email;
            if (body.is_staff.HasValue) u.IsStaff = body.is_staff.Value;
            if (body.is_active.HasValue) u.IsActive = body.is_active.Value;
            if (!string.IsNullOrEmpty(body.password)) u.Password = BCrypt.Net.BCrypt.HashPassword(body.password);
            u.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = u.Id, username = u.Username, email = u.Email, is_active = u.IsActive });
    }

    private static async Task<IResult> DeleteUser(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var u = await db.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (u == null) return Results.NotFound();
        u.IsActive = false;
        u.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Customer Tags ─────────────────────────────────────────────────────────

    private static async Task<IResult> CreateCustomerTag(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerTagCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var t = new CustomerTag { WorkspaceId = wid.Value, Name = body.name ?? "" };
        db.CustomerTags.Add(t);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/customer-tags/", new { id = t.Id, name = t.Name });
    }

    private static async Task<IResult> ListCustomerTags(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.CustomerTags.AsNoTracking();
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.Select(t => new { id = t.Id, name = t.Name }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetCustomerTag(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.CustomerTags.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        return Results.Ok(new { id = t.Id, name = t.Name, workspace = t.WorkspaceId });
    }

    private static async Task<IResult> PatchCustomerTag(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.CustomerTags.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerTagPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) t.Name = body.name;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = t.Id, name = t.Name });
    }

    private static async Task<IResult> DeleteCustomerTag(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var t = await db.CustomerTags.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (t == null) return Results.NotFound();
        db.CustomerTags.Remove(t);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> TagCustomers(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var tag = await db.CustomerTags.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (tag == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerIdsBody>(ct);
        if (body?.customer_ids == null) return Results.BadRequest();
        var existingLinks = await db.CustomerTagCustomers.Where(x => x.CustomertagId == id && body.customer_ids.Contains(x.CustomerId)).Select(x => x.CustomerId).ToListAsync(ct);
        foreach (var customerId in body.customer_ids.Except(existingLinks))
        {
            db.CustomerTagCustomers.Add(new CustomerTagCustomer { CustomertagId = id, CustomerId = customerId });
        }
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { tagged = body.customer_ids.Count });
    }

    private static async Task<IResult> UntagCustomers(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var tag = await db.CustomerTags.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (tag == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerIdsBody>(ct);
        if (body?.customer_ids == null) return Results.BadRequest();
        var links = await db.CustomerTagCustomers.Where(x => x.CustomertagId == id && body.customer_ids.Contains(x.CustomerId)).ToListAsync(ct);
        db.CustomerTagCustomers.RemoveRange(links);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { untagged = links.Count });
    }

    // ── Customer Segments ─────────────────────────────────────────────────────

    private static async Task<IResult> CreateCustomerSegment(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerSegmentCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var s = new CustomerSegment { WorkspaceId = wid.Value, Name = body.name ?? "", IsActive = true };
        db.CustomerSegments.Add(s);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/customer-segments/", new { id = s.Id, name = s.Name });
    }

    private static async Task<IResult> ListCustomerSegments(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.CustomerSegments.AsNoTracking();
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.Select(s => new { id = s.Id, name = s.Name, is_active = s.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> GetCustomerSegment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.CustomerSegments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        return Results.Ok(new { id = s.Id, name = s.Name, is_active = s.IsActive });
    }

    private static async Task<IResult> PatchCustomerSegment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.CustomerSegments.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<CustomerSegmentPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) s.Name = body.name;
            if (body.is_active.HasValue) s.IsActive = body.is_active.Value;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = s.Id, name = s.Name, is_active = s.IsActive });
    }

    private static async Task<IResult> DeleteCustomerSegment(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var s = await db.CustomerSegments.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (s == null) return Results.NotFound();
        s.IsActive = false;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── Staff Roles ───────────────────────────────────────────────────────────

    private static async Task<IResult> ListStaffRoles(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.StaffRoles.AsNoTracking().Where(r => r.IsActive);
        if (wid.HasValue) query = query.Where(r => r.WorkspaceId == wid.Value);
        var list = await query.Select(r => new { id = r.Id, name = r.Name, code = r.Code }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateStaffRole(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<StaffRoleCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var r = new StaffRole
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Code = body.code ?? "",
            Description = body.description ?? "",
            IsSystem = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.StaffRoles.Add(r);
        await db.SaveChangesAsync(ct);
        return Results.Created("/api/v1/staff-roles/", new { id = r.Id, name = r.Name, code = r.Code });
    }

    private static async Task<IResult> GetStaffRole(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var r = await db.StaffRoles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (r == null) return Results.NotFound();
        return Results.Ok(new { id = r.Id, name = r.Name, code = r.Code, description = r.Description, is_system = r.IsSystem, is_active = r.IsActive });
    }

    private static async Task<IResult> PatchStaffRole(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var r = await db.StaffRoles.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (r == null) return Results.NotFound();
        if (r.IsSystem) return Results.BadRequest(new { detail = "Cannot modify system roles." });
        var body = await ctx.Request.ReadFromJsonAsync<StaffRolePatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) r.Name = body.name;
            if (body.description != null) r.Description = body.description;
            r.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = r.Id, name = r.Name, code = r.Code });
    }

    private static async Task<IResult> DeleteStaffRole(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var r = await db.StaffRoles.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (r == null) return Results.NotFound();
        if (r.IsSystem) return Results.BadRequest(new { detail = "Cannot delete system roles." });
        r.IsActive = false;
        r.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    // ── API Keys ──────────────────────────────────────────────────────────────

    private static string GenerateRawApiKey() => "bfg_" + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).Replace("+", "").Replace("/", "").Replace("=", "")[..40];

    private static string HashApiKey(string rawKey)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static async Task<IResult> ListApiKeys(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.ApiKeys.AsNoTracking();
        if (wid.HasValue) query = query.Where(k => k.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(k => k.CreatedAt).Select(k => new { id = k.Id, name = k.Name, prefix = k.Prefix, permissions = k.Permissions, is_active = k.IsActive, created_at = k.CreatedAt, last_used_at = k.LastUsedAt }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> CreateApiKey(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        if (!wid.HasValue) return Results.BadRequest();
        var body = await ctx.Request.ReadFromJsonAsync<ApiKeyCreateBody>(ct);
        if (body == null) return Results.BadRequest();
        var rawKey = GenerateRawApiKey();
        var keyHash = HashApiKey(rawKey);
        var k = new ApiKey
        {
            WorkspaceId = wid.Value,
            Name = body.name ?? "",
            Prefix = rawKey[..8],
            KeyHash = keyHash,
            Permissions = body.permissions ?? "[]",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        db.ApiKeys.Add(k);
        await db.SaveChangesAsync(ct);
        // Return the full key only once
        return Results.Created("/api/v1/api-keys/", new { id = k.Id, name = k.Name, key = rawKey, prefix = k.Prefix, permissions = k.Permissions });
    }

    private static async Task<IResult> GetApiKey(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var k = await db.ApiKeys.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (k == null) return Results.NotFound();
        // Never return the key hash; return metadata only
        return Results.Ok(new { id = k.Id, name = k.Name, prefix = k.Prefix, permissions = k.Permissions, is_active = k.IsActive, created_at = k.CreatedAt, last_used_at = k.LastUsedAt });
    }

    private static async Task<IResult> PatchApiKey(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var k = await db.ApiKeys.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (k == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<ApiKeyPatchBody>(ct);
        if (body != null)
        {
            if (body.name != null) k.Name = body.name;
            if (body.permissions != null) k.Permissions = body.permissions;
            k.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = k.Id, name = k.Name, prefix = k.Prefix, permissions = k.Permissions });
    }

    private static async Task<IResult> DeleteApiKey(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var k = await db.ApiKeys.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (k == null) return Results.NotFound();
        db.ApiKeys.Remove(k);
        await db.SaveChangesAsync(ct);
        return Results.NoContent();
    }

    private static async Task<IResult> RegenerateApiKey(BfgDbContext db, HttpContext ctx, int id, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var k = await db.ApiKeys.FirstOrDefaultAsync(x => x.Id == id && (!wid.HasValue || x.WorkspaceId == wid.Value), ct);
        if (k == null) return Results.NotFound();
        var rawKey = GenerateRawApiKey();
        k.KeyHash = HashApiKey(rawKey);
        k.Prefix = rawKey[..8];
        k.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { id = k.Id, name = k.Name, key = rawKey, prefix = k.Prefix });
    }

    // ── Staff Members / Misc (existing) ───────────────────────────────────────

    private static async Task<IResult> ListStaffMembers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.StaffMembers.AsNoTracking();
        if (wid.HasValue) query = query.Where(m => m.WorkspaceId == wid.Value);
        var list = await query.Select(m => new { id = m.Id, user_id = m.UserId, role_id = m.RoleId }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> Me(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var customer = wid.HasValue ? await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.UserId == userId && c.WorkspaceId == wid.Value, ct) : null;
        return Results.Ok(new
        {
            id = user.Id,
            username = user.Username,
            email = user.Email,
            first_name = user.FirstName,
            last_name = user.LastName,
            phone = user.Phone,
            avatar = user.Avatar,
            default_workspace = user.DefaultWorkspaceId,
            language = user.Language,
            timezone_name = user.TimezoneName,
            is_staff = user.IsStaff,
            is_active = user.IsActive,
            date_joined = user.DateJoined,
            updated_at = user.UpdatedAt,
            customer = customer != null ? new { id = customer.Id, workspace = customer.WorkspaceId } : (object?)null
        });
    }

    private static async Task<IResult> PatchMe(HttpContext ctx, BfgDbContext db, CancellationToken ct)
    {
        var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? ctx.User.FindFirst("user_id")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user == null) return Results.NotFound();
        var body = await ctx.Request.ReadFromJsonAsync<MePatchBody>(ct);
        if (body != null)
        {
            if (body.first_name != null) user.FirstName = body.first_name;
            if (body.last_name != null) user.LastName = body.last_name;
            if (body.phone != null) user.Phone = body.phone;
            user.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(ct);
        }
        return Results.Ok(new { id = user.Id, first_name = user.FirstName, last_name = user.LastName, phone = user.Phone });
    }

    private static IResult Options()
    {
        return Results.Ok(new { });
    }

    private static IResult Countries()
    {
        var countries = new[] { new { code = "NZ", name = "New Zealand" }, new { code = "AU", name = "Australia" }, new { code = "US", name = "United States" }, new { code = "CN", name = "China" } };
        return Results.Ok(countries);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string Slugify(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "workspace";
        var slug = new string(name.ToLowerInvariant().Where(c => char.IsLetterOrDigit(c) || c == ' ' || c == '-').ToArray()).Trim();
        slug = string.Join("-", slug.Split(new[] { ' ', '-' }, StringSplitOptions.RemoveEmptyEntries));
        return slug.Length > 0 ? slug : "workspace";
    }

    private static async Task<int> GetMaxCustomerSequenceAsync(BfgDbContext db, int workspaceId, CancellationToken ct)
    {
        var numbers = await db.Customers.AsNoTracking().Where(c => c.WorkspaceId == workspaceId && c.CustomerNumber.StartsWith("CUST-")).Select(c => c.CustomerNumber).ToListAsync(ct);
        var max = 0;
        foreach (var n in numbers)
        {
            if (n.Length >= 10 && int.TryParse(n.AsSpan(5), out var seq)) max = Math.Max(max, seq);
        }
        return max;
    }

    // ── Request body records ──────────────────────────────────────────────────

    private sealed record WorkspaceCreateBody(string? name, string? slug, string? domain, string? email);
    private sealed record WorkspacePatchBody(string? name, string? email, string? phone);
    private sealed record CustomerCreateBody(int user, int user_id, string? company_name, string? tax_number);
    private sealed record CustomerPatchBody(string? company_name, string? tax_number, bool? is_active);
    private sealed record AddressCreateBody(string? full_name, string? phone, string? email, string? address_line1, string? address_line2, string? city, string? state, string? postal_code, string? country, bool? is_default);
    private sealed record AddressPatchBody(string? full_name, string? phone, string? email, string? address_line1, string? address_line2, string? city, string? state, string? postal_code, string? country, bool? is_default);
    private sealed record SettingsPatchBody(string? site_name, string? default_language, string? default_currency, string? contact_email);
    private sealed record EmailConfigCreateBody(string? name, string? backend_type, string? host, int? port, string? username, string? password, bool? use_tls, string? from_email, bool? is_default);
    private sealed record EmailConfigPatchBody(string? name, string? host, int? port, string? username, string? password, bool? use_tls, string? from_email, bool? is_active);
    private sealed record UserCreateBody(string? username, string? email, string? first_name, string? last_name, string? phone, string? password, bool? is_staff);
    private sealed record UserPatchBody(string? first_name, string? last_name, string? phone, string? email, bool? is_staff, bool? is_active, string? password);
    private sealed record CustomerTagCreateBody(string? name);
    private sealed record CustomerTagPatchBody(string? name);
    private sealed record CustomerIdsBody(List<int> customer_ids);
    private sealed record CustomerSegmentCreateBody(string? name, string? description);
    private sealed record CustomerSegmentPatchBody(string? name, string? description, bool? is_active);
    private sealed record StaffRoleCreateBody(string? name, string? code, string? description);
    private sealed record StaffRolePatchBody(string? name, string? description);
    private sealed record ApiKeyCreateBody(string? name, string? permissions);
    private sealed record ApiKeyPatchBody(string? name, string? permissions);
    private sealed record MePatchBody(string? first_name, string? last_name, string? phone);
}
