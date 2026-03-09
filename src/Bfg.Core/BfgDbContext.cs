using Bfg.Core.Common;
using Bfg.Core.Shop;
using Bfg.Core.Web;
using Microsoft.EntityFrameworkCore;

namespace Bfg.Core;

public class BfgDbContext : DbContext
{
    public BfgDbContext(DbContextOptions<BfgDbContext> options) : base(options) { }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Settings> Settings => Set<Settings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<MediaLink> MediaLinks => Set<MediaLink>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerSegment> CustomerSegments => Set<CustomerSegment>();
    public DbSet<CustomerTag> CustomerTags => Set<CustomerTag>();
    public DbSet<CustomerTagCustomer> CustomerTagCustomers => Set<CustomerTagCustomer>();
    public DbSet<StaffRole> StaffRoles => Set<StaffRole>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<EmailConfig> EmailConfigs => Set<EmailConfig>();

    public DbSet<Site> WebSites => Set<Site>();
    public DbSet<Theme> WebThemes => Set<Theme>();
    public DbSet<Language> WebLanguages => Set<Language>();
    public DbSet<Page> WebPages => Set<Page>();
    public DbSet<Inquiry> WebInquiries => Set<Inquiry>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Table names match Django app_model (common_workspace, common_user, ...)
        modelBuilder.Entity<Workspace>().ToTable("common_workspace");
        modelBuilder.Entity<User>().ToTable("common_user");
        modelBuilder.Entity<Address>().ToTable("common_address");
        modelBuilder.Entity<Settings>().ToTable("common_settings");
        modelBuilder.Entity<AuditLog>().ToTable("common_auditlog");
        modelBuilder.Entity<Media>().ToTable("common_media");
        modelBuilder.Entity<MediaLink>().ToTable("common_medialink");
        modelBuilder.Entity<Customer>().ToTable("common_customer");
        modelBuilder.Entity<CustomerSegment>().ToTable("common_customersegment");
        modelBuilder.Entity<CustomerTag>().ToTable("common_customertag");
        modelBuilder.Entity<CustomerTagCustomer>().ToTable("common_customertag_customers");
        modelBuilder.Entity<StaffRole>().ToTable("common_staffrole");
        modelBuilder.Entity<StaffMember>().ToTable("common_staffmember");
        modelBuilder.Entity<UserPreferences>().ToTable("common_userpreferences");
        modelBuilder.Entity<EmailConfig>().ToTable("common_emailconfig");

        modelBuilder.Entity<Site>().ToTable("web_site");
        modelBuilder.Entity<Theme>().ToTable("web_theme");
        modelBuilder.Entity<Language>().ToTable("web_language");
        modelBuilder.Entity<Page>().ToTable("web_page");
        modelBuilder.Entity<Inquiry>().ToTable("web_inquiry");

        modelBuilder.Entity<Product>().ToTable("shop_product");
        modelBuilder.Entity<ProductCategory>().ToTable("shop_productcategory");
        modelBuilder.Entity<Store>().ToTable("shop_store");
        modelBuilder.Entity<Cart>().ToTable("shop_cart");
        modelBuilder.Entity<Order>().ToTable("shop_order");

        // Column names: Django uses snake_case. EF default is PascalCase; Npgsql can use snake_case via convention.
        // Configure key FKs to use snake_case so migrations match Django.
        modelBuilder.Entity<User>()
            .Property(u => u.DefaultWorkspaceId)
            .HasColumnName("default_workspace_id");
        modelBuilder.Entity<Address>()
            .Property(a => a.WorkspaceId)
            .HasColumnName("workspace_id");
        modelBuilder.Entity<Address>()
            .Property(a => a.ContentTypeId)
            .HasColumnName("content_type_id");
        modelBuilder.Entity<Address>()
            .Property(a => a.ObjectId)
            .HasColumnName("object_id");
        modelBuilder.Entity<CustomerTagCustomer>()
            .Property(x => x.CustomertagId)
            .HasColumnName("customertag_id");
        modelBuilder.Entity<CustomerTagCustomer>()
            .Property(x => x.CustomerId)
            .HasColumnName("customer_id");

        // Unique constraints to match Django
        modelBuilder.Entity<Workspace>().HasIndex(w => w.Slug).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.WorkspaceId, c.UserId })
            .IsUnique();
        modelBuilder.Entity<CustomerTag>()
            .HasIndex(t => new { t.WorkspaceId, t.Name })
            .IsUnique();
        modelBuilder.Entity<StaffMember>()
            .HasIndex(m => new { m.WorkspaceId, m.UserId })
            .IsUnique();
        modelBuilder.Entity<UserPreferences>()
            .HasIndex(p => p.UserId)
            .IsUnique();
    }
}
