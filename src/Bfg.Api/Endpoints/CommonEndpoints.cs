using System.Security.Claims;
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

        group.MapGet("/workspaces", ListWorkspaces);
        group.MapPost("/workspaces/", CreateWorkspace);
        group.MapGet("/customers", ListCustomers);
        group.MapPost("/customers/", CreateCustomer);
        group.MapGet("/customers/{id:int}", GetCustomer);
        group.MapGet("/addresses", ListAddresses);
        group.MapPost("/addresses/", CreateAddress);
        group.MapGet("/addresses/{id:int}", GetAddress);
        group.MapGet("/settings", ListSettings);
        group.MapGet("/email-configs", ListEmailConfigs);
        group.MapGet("/users", ListUsers);
        group.MapPost("/customer-segments/", CreateCustomerSegment);
        group.MapGet("/customer-segments", ListCustomerSegments);
        group.MapPost("/customer-tags/", CreateCustomerTag);
        group.MapGet("/customer-tags", ListCustomerTags);
        group.MapGet("/staff-roles", ListStaffRoles);
        group.MapGet("/staff-members", ListStaffMembers);
        // /me is handled by MeEndpoints (/api/v1/me/)
        group.MapGet("/options/", Options);
        group.MapGet("/countries/", Countries);
    }

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

    private static async Task<IResult> ListCustomers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Customers.AsNoTracking();
        if (wid.HasValue) query = query.Where(c => c.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(c => c.CreatedAt).Select(c => new { id = c.Id, customer_number = c.CustomerNumber, is_active = c.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListAddresses(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Addresses.AsNoTracking();
        if (wid.HasValue) query = query.Where(a => a.WorkspaceId == wid.Value);
        var list = await query.OrderByDescending(a => a.CreatedAt).Select(a => new { id = a.Id, full_name = a.FullName, city = a.City, country = a.Country }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListSettings(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var list = await db.Settings.AsNoTracking().Where(s => wid == null || s.WorkspaceId == wid.Value).Select(s => new { id = s.Id, workspace_id = s.WorkspaceId, site_name = s.SiteName }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListEmailConfigs(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.EmailConfigs.AsNoTracking();
        if (wid.HasValue) query = query.Where(e => e.WorkspaceId == wid.Value);
        var list = await query.Select(e => new { id = e.Id, name = e.Name, backend_type = e.BackendType, is_default = e.IsDefault }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListUsers(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.Users.AsNoTracking();
        if (wid.HasValue) query = query.Where(u => u.DefaultWorkspaceId == wid.Value);
        var list = await query.Select(u => new { id = u.Id, username = u.Username, email = u.Email, is_active = u.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListCustomerSegments(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.CustomerSegments.AsNoTracking();
        if (wid.HasValue) query = query.Where(s => s.WorkspaceId == wid.Value);
        var list = await query.Select(s => new { id = s.Id, name = s.Name, is_active = s.IsActive }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListCustomerTags(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.CustomerTags.AsNoTracking();
        if (wid.HasValue) query = query.Where(t => t.WorkspaceId == wid.Value);
        var list = await query.Select(t => new { id = t.Id, name = t.Name }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private static async Task<IResult> ListStaffRoles(BfgDbContext db, HttpContext ctx, CancellationToken ct)
    {
        var wid = WorkspaceMiddleware.GetWorkspaceId(ctx);
        var query = db.StaffRoles.AsNoTracking().Where(r => r.IsActive);
        if (wid.HasValue) query = query.Where(r => r.WorkspaceId == wid.Value);
        var list = await query.Select(r => new { id = r.Id, name = r.Name, code = r.Code }).ToListAsync(ct);
        return Results.Ok(list);
    }

    private sealed record WorkspaceCreateBody(string? name, string? slug, string? domain, string? email);
    private sealed record CustomerCreateBody(int user, int user_id, string? company_name, string? tax_number);
    private sealed record AddressCreateBody(string? full_name, string? phone, string? email, string? address_line1, string? address_line2, string? city, string? state, string? postal_code, string? country, bool? is_default);
    private sealed record CustomerSegmentCreateBody(string? name, string? description);
    private sealed record CustomerTagCreateBody(string? name);
    private sealed record MePatchBody(string? first_name, string? last_name, string? phone);
}
