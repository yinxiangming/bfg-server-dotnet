using Bfg.Core.Common;
using Bfg.Core.Delivery;
using Bfg.Core.Finance;
using Bfg.Core.Inbox;
using Bfg.Core.Promo;
using Bfg.Core.Shop;
using Bfg.Core.Support;
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
    public DbSet<ProductCategoryProduct> ProductCategoryProducts => Set<ProductCategoryProduct>();
    public DbSet<Variant> Variants => Set<Variant>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreWarehouse> StoreWarehouses => Set<StoreWarehouse>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<FreightService> FreightServices => Set<FreightService>();
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<Shipment> Shipments => Set<Shipment>();

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();

    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<DiscountRule> DiscountRules => Set<DiscountRule>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();

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
        modelBuilder.Entity<ProductCategoryProduct>().ToTable("shop_product_categories").HasKey(x => new { x.ProductId, x.ProductCategoryId });
        modelBuilder.Entity<Variant>().ToTable("shop_productvariant");
        modelBuilder.Entity<Store>().ToTable("shop_store");
        modelBuilder.Entity<StoreWarehouse>().ToTable("shop_store_warehouses").HasKey(x => new { x.StoreId, x.WarehouseId });
        modelBuilder.Entity<Cart>().ToTable("shop_cart");
        modelBuilder.Entity<CartItem>().ToTable("shop_cartitem");
        modelBuilder.Entity<Order>().ToTable("shop_order");
        modelBuilder.Entity<OrderItem>().ToTable("shop_orderitem");

        modelBuilder.Entity<Warehouse>().ToTable("delivery_warehouse");
        modelBuilder.Entity<Carrier>().ToTable("delivery_carrier");
        modelBuilder.Entity<FreightService>().ToTable("delivery_freightservice");
        modelBuilder.Entity<DeliveryZone>().ToTable("delivery_deliveryzone");
        modelBuilder.Entity<Shipment>().ToTable("delivery_shipment");

        modelBuilder.Entity<Currency>().ToTable("finance_currency");
        modelBuilder.Entity<PaymentGateway>().ToTable("finance_paymentgateway");
        modelBuilder.Entity<Payment>().ToTable("finance_payment");
        modelBuilder.Entity<PaymentMethod>().ToTable("finance_paymentmethod");
        modelBuilder.Entity<Invoice>().ToTable("finance_invoice");

        modelBuilder.Entity<SupportTicket>().ToTable("support_supportticket");
        modelBuilder.Entity<TicketMessage>().ToTable("support_ticketmessage");

        modelBuilder.Entity<InboxMessage>().ToTable("inbox_message");
        modelBuilder.Entity<MessageTemplate>().ToTable("inbox_messagetemplate");
        modelBuilder.Entity<Notification>().ToTable("inbox_notification");

        modelBuilder.Entity<Voucher>().ToTable("marketing_voucher");
        modelBuilder.Entity<Campaign>().ToTable("marketing_campaign");
        modelBuilder.Entity<DiscountRule>().ToTable("marketing_discountrule");
        modelBuilder.Entity<GiftCard>().ToTable("marketing_giftcard");

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
