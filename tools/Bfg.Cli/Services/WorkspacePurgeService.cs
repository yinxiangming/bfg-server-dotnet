using Bfg.Core;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Cli.Services;

/// <summary>
/// Deletes all tenant-scoped rows for one workspace via EF Core bulk APIs.
/// Does not delete global rows (e.g. finance_currency, common_user).
/// Extend when new workspace-scoped tables are added in migrations.
/// </summary>
public static class WorkspacePurgeService
{
    public static async Task PurgeAsync(BfgDbContext db, int workspaceId, CancellationToken ct = default)
    {
        var wid = workspaceId;

        await db.Users
            .Where(u => u.DefaultWorkspaceId == wid)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.DefaultWorkspaceId, (int?)null), ct);

        await db.TicketMessages
            .Where(m => db.SupportTickets.Any(t => t.Id == m.TicketId && t.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.SupportTickets.Where(t => t.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.InboxMessages.Where(m => m.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Notifications.Where(n => n.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.MessageTemplates.Where(m => m.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.Vouchers.Where(v => v.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Campaigns.Where(c => c.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.DiscountRules.Where(d => d.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.GiftCards.Where(g => g.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.Payments.Where(p => p.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Invoices.Where(i => i.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.PaymentMethods
            .Where(pm => pm.WorkspaceId == wid
                         || db.PaymentGateways.Any(g => g.Id == pm.GatewayId && g.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.PaymentGateways.Where(g => g.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.Shipments.Where(s => s.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.CartItems
            .Where(ci => db.Carts.Any(c => c.Id == ci.CartId && c.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.Carts.Where(c => c.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.OrderItems
            .Where(oi => db.Orders.Any(o => o.Id == oi.OrderId && o.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.Orders.Where(o => o.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.ProductCategoryProducts
            .Where(pcp => db.Products.Any(p => p.Id == pcp.ProductId && p.WorkspaceId == wid)
                          || db.ProductCategories.Any(c => c.Id == pcp.ProductCategoryId && c.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);

        await db.Variants
            .Where(v => db.Products.Any(p => p.Id == v.ProductId && p.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.Products.Where(p => p.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await DeleteProductCategoriesTreeAsync(db, wid, ct);

        await db.StoreWarehouses
            .Where(sw => db.Stores.Any(s => s.Id == sw.StoreId && s.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.Stores.Where(s => s.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.FreightServices.Where(f => f.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Carriers.Where(c => c.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.DeliveryZones.Where(z => z.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Warehouses.Where(w => w.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.CustomerTagCustomers
            .Where(x => db.Customers.Any(c => c.Id == x.CustomerId && c.WorkspaceId == wid)
                        || db.CustomerTags.Any(t => t.Id == x.CustomertagId && t.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.CustomerTags.Where(t => t.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.CustomerSegments.Where(s => s.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Customers.Where(c => c.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.Addresses.Where(a => a.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.StaffMembers.Where(m => m.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.StaffRoles.Where(r => r.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.MediaLinks
            .Where(ml => db.Media.Any(m => m.Id == ml.MediaId && m.WorkspaceId == wid))
            .ExecuteDeleteAsync(ct);
        await db.Media.Where(m => m.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await DeleteWebPagesTreeAsync(db, wid, ct);
        await db.WebLanguages.Where(l => l.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.WebSites.Where(s => s.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.WebThemes.Where(t => t.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.WebInquiries.Where(i => i.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.AuditLogs.Where(a => a.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.Settings.Where(s => s.WorkspaceId == wid).ExecuteDeleteAsync(ct);
        await db.EmailConfigs.Where(e => e.WorkspaceId == wid).ExecuteDeleteAsync(ct);

        await db.Workspaces.Where(w => w.Id == wid).ExecuteDeleteAsync(ct);
    }

    /// <summary>
    /// Purge every workspace. Snapshots ids up front (each <see cref="PurgeAsync"/> removes one workspace row).
    /// Never deletes <c>common_user</c> (superadmin / all logins remain; <c>StaffMember</c> per workspace is removed).
    /// </summary>
    public static async Task PurgeAllWorkspacesAsync(BfgDbContext db, CancellationToken ct = default)
    {
        var ids = await db.Workspaces.AsNoTracking().Select(w => w.Id).OrderBy(id => id).ToListAsync(ct);
        foreach (var id in ids)
            await PurgeAsync(db, id, ct);
    }

    private static async Task DeleteProductCategoriesTreeAsync(BfgDbContext db, int wid, CancellationToken ct)
    {
        while (await db.ProductCategories.AnyAsync(c => c.WorkspaceId == wid, ct))
        {
            var n = await db.ProductCategories
                .Where(c => c.WorkspaceId == wid
                            && !db.ProductCategories.Any(ch => ch.ParentId == c.Id && ch.WorkspaceId == wid))
                .ExecuteDeleteAsync(ct);
            if (n == 0)
                throw new InvalidOperationException(
                    $"Cannot clear product categories for workspace {wid} (check ParentId / orphaned rows).");
        }
    }

    private static async Task DeleteWebPagesTreeAsync(BfgDbContext db, int wid, CancellationToken ct)
    {
        while (await db.WebPages.AnyAsync(p => p.WorkspaceId == wid, ct))
        {
            var n = await db.WebPages
                .Where(p => p.WorkspaceId == wid
                            && !db.WebPages.Any(ch => ch.ParentId == p.Id && ch.WorkspaceId == wid))
                .ExecuteDeleteAsync(ct);
            if (n == 0)
                throw new InvalidOperationException(
                    $"Cannot clear web pages for workspace {wid} (check ParentId / orphaned rows).");
        }
    }
}
