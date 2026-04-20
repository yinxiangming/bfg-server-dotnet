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

    // Common
    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Settings> Settings => Set<Settings>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Media> Media => Set<Media>();
    public DbSet<MediaLink> MediaLinks => Set<MediaLink>();
    public DbSet<DjangoContentType> DjangoContentTypes => Set<DjangoContentType>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerSegment> CustomerSegments => Set<CustomerSegment>();
    public DbSet<CustomerTag> CustomerTags => Set<CustomerTag>();
    public DbSet<CustomerTagCustomer> CustomerTagCustomers => Set<CustomerTagCustomer>();
    public DbSet<StaffRole> StaffRoles => Set<StaffRole>();
    public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();
    public DbSet<EmailConfig> EmailConfigs => Set<EmailConfig>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    // Web / CMS
    public DbSet<Site> WebSites => Set<Site>();
    public DbSet<Theme> WebThemes => Set<Theme>();
    public DbSet<Language> WebLanguages => Set<Language>();
    public DbSet<Page> WebPages => Set<Page>();
    public DbSet<Inquiry> WebInquiries => Set<Inquiry>();
    public DbSet<Post> WebPosts => Set<Post>();
    public DbSet<WebCategory> WebCategories => Set<WebCategory>();
    public DbSet<WebTag> WebTags => Set<WebTag>();
    public DbSet<Menu> WebMenus => Set<Menu>();
    public DbSet<MenuItem> WebMenuItems => Set<MenuItem>();
    public DbSet<NewsletterSubscription> NewsletterSubscriptions => Set<NewsletterSubscription>();
    public DbSet<NewsletterTemplate> NewsletterTemplates => Set<NewsletterTemplate>();
    public DbSet<NewsletterSend> NewsletterSends => Set<NewsletterSend>();
    public DbSet<BookingTimeSlot> BookingTimeSlots => Set<BookingTimeSlot>();
    public DbSet<Booking> Bookings => Set<Booking>();

    // Shop
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductCategoryProduct> ProductCategoryProducts => Set<ProductCategoryProduct>();
    public DbSet<Variant> Variants => Set<Variant>();
    public DbSet<SalesChannel> SalesChannels => Set<SalesChannel>();
    public DbSet<ProductChannelListing> ProductChannelListings => Set<ProductChannelListing>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreWarehouse> StoreWarehouses => Set<StoreWarehouse>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Return> Returns => Set<Return>();
    public DbSet<ReturnItem> ReturnItems => Set<ReturnItem>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionProduct> CollectionProducts => Set<CollectionProduct>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ProductTag> ProductTags => Set<ProductTag>();
    public DbSet<ProductTagProduct> ProductTagProducts => Set<ProductTagProduct>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();

    // Delivery
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<FreightService> FreightServices => Set<FreightService>();
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<FreightStatus> FreightStatuses => Set<FreightStatus>();
    public DbSet<PackageTemplate> PackageTemplates => Set<PackageTemplate>();
    public DbSet<DeliveryPackage> DeliveryPackages => Set<DeliveryPackage>();
    public DbSet<Consignment> Consignments => Set<Consignment>();
    public DbSet<ConsignmentOrder> ConsignmentOrders => Set<ConsignmentOrder>();
    public DbSet<PackagingType> PackagingTypes => Set<PackagingType>();
    public DbSet<TrackingEvent> TrackingEvents => Set<TrackingEvent>();

    // Finance
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<PaymentGateway> PaymentGateways => Set<PaymentGateway>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceItem> InvoiceItems => Set<InvoiceItem>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<FinancialCode> FinancialCodes => Set<FinancialCode>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<WithdrawalRequest> WithdrawalRequests => Set<WithdrawalRequest>();

    // Support
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();
    public DbSet<TicketCategory> TicketCategories => Set<TicketCategory>();
    public DbSet<TicketPriority> TicketPriorities => Set<TicketPriority>();

    // Inbox
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<MessageRecipient> MessageRecipients => Set<MessageRecipient>();

    // Marketing / Promo
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<DiscountRule> DiscountRules => Set<DiscountRule>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<CampaignDisplay> CampaignDisplays => Set<CampaignDisplay>();
    public DbSet<CampaignParticipation> CampaignParticipations => Set<CampaignParticipation>();
    public DbSet<ReferralProgram> ReferralPrograms => Set<ReferralProgram>();
    public DbSet<StampRecord> StampRecords => Set<StampRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ── Common ─────────────────────────────────────────────────────────────
        modelBuilder.Entity<Workspace>().ToTable("common_workspace");
        modelBuilder.Entity<User>().ToTable("common_user");
        modelBuilder.Entity<Address>().ToTable("common_address");
        modelBuilder.Entity<Settings>().ToTable("common_settings");
        modelBuilder.Entity<AuditLog>().ToTable("common_auditlog");
        modelBuilder.Entity<Media>().ToTable("common_media");
        modelBuilder.Entity<MediaLink>().ToTable("common_medialink");
        modelBuilder.Entity<DjangoContentType>().ToTable("django_content_type");
        modelBuilder.Entity<DjangoContentType>().HasNoKey();
        modelBuilder.Entity<Customer>().ToTable("common_customer");
        modelBuilder.Entity<CustomerSegment>().ToTable("common_customersegment");
        modelBuilder.Entity<CustomerTag>().ToTable("common_customertag");
        modelBuilder.Entity<CustomerTagCustomer>().ToTable("common_customertag_customers");
        modelBuilder.Entity<StaffRole>().ToTable("common_staffrole");
        modelBuilder.Entity<StaffMember>().ToTable("common_staffmember");
        modelBuilder.Entity<UserPreferences>().ToTable("common_userpreferences");
        modelBuilder.Entity<EmailConfig>().ToTable("common_emailconfig");
        modelBuilder.Entity<ApiKey>().ToTable("common_apikey");
        modelBuilder.Entity<ApiKey>().Property(k => k.Permissions).HasColumnType("json");

        // ── Web / CMS ──────────────────────────────────────────────────────────
        modelBuilder.Entity<Site>().ToTable("web_site");
        modelBuilder.Entity<Theme>().ToTable("web_theme");
        modelBuilder.Entity<Language>().ToTable("web_language");
        modelBuilder.Entity<Page>().ToTable("web_page");
        modelBuilder.Entity<Page>().Property(p => p.SortOrder).HasColumnName("order");
        modelBuilder.Entity<Page>().Property(p => p.Blocks).HasColumnType("json");
        modelBuilder.Entity<Inquiry>().ToTable("web_inquiry");
        modelBuilder.Entity<Post>().ToTable("web_post");
        modelBuilder.Entity<WebCategory>().ToTable("web_category");
        modelBuilder.Entity<WebCategory>().Property(c => c.SortOrder).HasColumnName("order");
        modelBuilder.Entity<WebTag>().ToTable("web_tag");
        modelBuilder.Entity<Menu>().ToTable("web_menu");
        modelBuilder.Entity<MenuItem>().ToTable("web_menuitem");
        modelBuilder.Entity<MenuItem>().Property(m => m.SortOrder).HasColumnName("order");
        modelBuilder.Entity<NewsletterSubscription>().ToTable("web_newslettersubscription");
        modelBuilder.Entity<NewsletterTemplate>().ToTable("web_newslettertemplate");
        modelBuilder.Entity<NewsletterSend>().ToTable("web_newslettersend");
        modelBuilder.Entity<BookingTimeSlot>().ToTable("web_bookingtimeslot");
        modelBuilder.Entity<BookingTimeSlot>().Property(t => t.Date).HasColumnType("date");
        modelBuilder.Entity<Booking>().ToTable("web_booking");

        // ── Shop ───────────────────────────────────────────────────────────────
        modelBuilder.Entity<Product>().ToTable("shop_product");
        modelBuilder.Entity<ProductCategory>().ToTable("shop_productcategory");
        modelBuilder.Entity<ProductCategoryProduct>().ToTable("shop_product_categories").HasKey(x => new { x.ProductId, x.ProductCategoryId });
        modelBuilder.Entity<ProductCategoryProduct>().Property(x => x.ProductCategoryId).HasColumnName("productcategory_id");
        modelBuilder.Entity<ProductCategory>().Property(x => x.SortOrder).HasColumnName("order");
        modelBuilder.Entity<ProductCategory>().Property(x => x.Rules).HasColumnType("json");
        modelBuilder.Entity<Variant>().ToTable("shop_productvariant");
        modelBuilder.Entity<Variant>().Property(v => v.SortOrder).HasColumnName("order");
        modelBuilder.Entity<Variant>().Property(v => v.Options).HasColumnType("json");
        modelBuilder.Entity<SalesChannel>().ToTable("shop_saleschannel");
        modelBuilder.Entity<SalesChannel>().Property(s => s.Config).HasColumnType("json");
        modelBuilder.Entity<ProductChannelListing>().ToTable("shop_productchannellisting");
        modelBuilder.Entity<ProductChannelListing>().HasIndex(l => new { l.ProductId, l.ChannelId }).IsUnique();
        modelBuilder.Entity<Store>().ToTable("shop_store");
        modelBuilder.Entity<StoreWarehouse>().ToTable("shop_store_warehouses").HasKey(x => new { x.StoreId, x.WarehouseId });
        modelBuilder.Entity<Cart>().ToTable("shop_cart");
        modelBuilder.Entity<CartItem>().ToTable("shop_cartitem");
        modelBuilder.Entity<CartItem>().Property(i => i.UnitPrice).HasColumnName("price");
        modelBuilder.Entity<Order>().ToTable("shop_order");
        modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasColumnName("total");
        modelBuilder.Entity<Order>().Property(o => o.FulfillmentMethod).HasColumnName("fulfillment_method").IsRequired().HasDefaultValue("shipping");
        modelBuilder.Entity<Order>().Property(o => o.CustomerNote).IsRequired().HasDefaultValue("");
        modelBuilder.Entity<Order>().Property(o => o.AdminNote).IsRequired().HasDefaultValue("");
        modelBuilder.Entity<OrderItem>().ToTable("shop_orderitem");
        modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasColumnName("price");
        modelBuilder.Entity<OrderItem>().Property(i => i.TotalPrice).HasColumnName("subtotal");
        modelBuilder.Entity<OrderItem>().Ignore(i => i.CreatedAt);
        modelBuilder.Entity<Return>().ToTable("shop_return");
        modelBuilder.Entity<ReturnItem>().ToTable("shop_returnlineitem");
        modelBuilder.Entity<Collection>().ToTable("shop_collection");
        modelBuilder.Entity<CollectionProduct>().ToTable("shop_collection_products").HasKey(x => new { x.CollectionId, x.ProductId });
        modelBuilder.Entity<Wishlist>().ToTable("shop_wishlist");
        modelBuilder.Entity<WishlistItem>().ToTable("shop_wishlistitem");
        modelBuilder.Entity<ProductTag>().ToTable("shop_producttag");
        modelBuilder.Entity<ProductTagProduct>().ToTable("shop_product_tags");
        modelBuilder.Entity<ProductTagProduct>().Property(x => x.ProductTagId).HasColumnName("producttag_id");
        modelBuilder.Entity<ProductReview>().ToTable("shop_productreview");
        modelBuilder.Entity<ProductReview>().Property(r => r.Images).HasColumnType("json");

        // ── Delivery ───────────────────────────────────────────────────────────
        modelBuilder.Entity<Warehouse>().ToTable("delivery_warehouse");
        modelBuilder.Entity<Carrier>().ToTable("delivery_carrier");
        modelBuilder.Entity<Carrier>().Property(c => c.Config).HasColumnType("json");
        modelBuilder.Entity<Carrier>().Property(c => c.TestConfig).HasColumnType("json");
        modelBuilder.Entity<FreightService>().ToTable("delivery_freightservice");
        modelBuilder.Entity<FreightService>().Property(f => f.Config).HasColumnType("json");
        modelBuilder.Entity<FreightService>().Property(f => f.SortOrder).HasColumnName("order");
        modelBuilder.Entity<DeliveryZone>().ToTable("delivery_deliveryzone");
        modelBuilder.Entity<Shipment>().ToTable("delivery_shipment");
        modelBuilder.Entity<FreightStatus>().ToTable("delivery_freightstatus");
        modelBuilder.Entity<FreightStatus>().Property(f => f.SortOrder).HasColumnName("order");
        modelBuilder.Entity<PackageTemplate>().ToTable("delivery_packagetemplate");
        modelBuilder.Entity<PackageTemplate>().Property(t => t.SortOrder).HasColumnName("order");
        modelBuilder.Entity<DeliveryPackage>().ToTable("delivery_package");
        modelBuilder.Entity<Consignment>().ToTable("delivery_consignment");
        modelBuilder.Entity<ConsignmentOrder>().ToTable("delivery_consignment_orders");
        modelBuilder.Entity<PackagingType>().ToTable("delivery_packagingtype");
        modelBuilder.Entity<PackagingType>().Property(p => p.SortOrder).HasColumnName("order");
        modelBuilder.Entity<TrackingEvent>().ToTable("delivery_trackingevent");

        // ── Finance ────────────────────────────────────────────────────────────
        modelBuilder.Entity<Currency>().ToTable("finance_currency");
        modelBuilder.Entity<PaymentGateway>().ToTable("finance_paymentgateway");
        modelBuilder.Entity<Payment>().ToTable("finance_payment");
        modelBuilder.Entity<Payment>().Property(p => p.GatewayResponse).HasColumnType("json");
        modelBuilder.Entity<PaymentGateway>().Property(g => g.Config).HasColumnType("json");
        modelBuilder.Entity<PaymentGateway>().Property(g => g.TestConfig).HasColumnType("json");
        modelBuilder.Entity<PaymentMethod>().ToTable("finance_paymentmethod");
        modelBuilder.Entity<Invoice>().ToTable("finance_invoice");
        modelBuilder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnName("total");
        modelBuilder.Entity<Invoice>().Property(i => i.IssueDate).HasColumnName("issue_date").HasColumnType("date");
        modelBuilder.Entity<Invoice>().Property(i => i.DueDate).HasColumnName("due_date").HasColumnType("date");
        modelBuilder.Entity<Invoice>().Property(i => i.PaidDate).HasColumnName("paid_date").HasColumnType("date");
        modelBuilder.Entity<InvoiceItem>().ToTable("finance_invoiceitem");
        modelBuilder.Entity<Brand>().ToTable("finance_brand");
        modelBuilder.Entity<FinancialCode>().ToTable("finance_financialcode");
        modelBuilder.Entity<TaxRate>().ToTable("finance_taxrate");
        modelBuilder.Entity<Wallet>().ToTable("finance_wallet");
        modelBuilder.Entity<Transaction>().ToTable("finance_transaction");
        modelBuilder.Entity<WithdrawalRequest>().ToTable("finance_withdrawalrequest");

        // ── Support ────────────────────────────────────────────────────────────
        modelBuilder.Entity<SupportTicket>().ToTable("support_supportticket");
        modelBuilder.Entity<SupportTicket>().Property(t => t.RelatedOrderId).HasColumnName("related_order_id");
        modelBuilder.Entity<TicketMessage>().ToTable("support_ticketmessage");
        modelBuilder.Entity<TicketMessage>().Property(m => m.Body).HasColumnName("message");
        modelBuilder.Entity<TicketMessage>().Property(m => m.UserId).HasColumnName("sender_id");
        modelBuilder.Entity<TicketMessage>().Property(m => m.IsStaffReply).HasColumnName("is_staff_reply");
        modelBuilder.Entity<TicketCategory>().ToTable("support_ticketcategory");
        modelBuilder.Entity<TicketCategory>().Property(c => c.SortOrder).HasColumnName("order");
        modelBuilder.Entity<TicketPriority>().ToTable("support_ticketpriority");

        // ── Inbox ──────────────────────────────────────────────────────────────
        modelBuilder.Entity<InboxMessage>().ToTable("inbox_message");
        modelBuilder.Entity<MessageTemplate>().ToTable("inbox_messagetemplate");
        modelBuilder.Entity<MessageTemplate>().Property(t => t.AvailableVariables).HasColumnType("json");
        modelBuilder.Entity<Notification>().ToTable("inbox_notification");
        modelBuilder.Entity<MessageRecipient>().ToTable("inbox_messagerecipient");

        // ── Marketing / Promo ──────────────────────────────────────────────────
        modelBuilder.Entity<Voucher>().ToTable("marketing_coupon");
        modelBuilder.Entity<Voucher>().Property(v => v.TimesUsed).HasColumnName("times_used");
        modelBuilder.Entity<Campaign>().ToTable("marketing_campaign");
        modelBuilder.Entity<Campaign>().Property(c => c.Config).HasColumnType("json");
        modelBuilder.Entity<DiscountRule>().ToTable("marketing_discountrule");
        modelBuilder.Entity<DiscountRule>().Property(r => r.Config).HasColumnType("json");
        modelBuilder.Entity<DiscountRule>().Property(r => r.PrerequisiteProductIds).HasColumnType("json");
        modelBuilder.Entity<DiscountRule>().Property(r => r.EntitledProductIds).HasColumnType("json");
        modelBuilder.Entity<GiftCard>().ToTable("marketing_giftcard");
        modelBuilder.Entity<CampaignDisplay>().ToTable("marketing_campaigndisplay");
        modelBuilder.Entity<CampaignDisplay>().Property(d => d.SortOrder).HasColumnName("order");
        modelBuilder.Entity<CampaignDisplay>().Property(d => d.Rules).HasColumnType("json");
        modelBuilder.Entity<CampaignParticipation>().ToTable("marketing_campaignparticipation");
        modelBuilder.Entity<ReferralProgram>().ToTable("marketing_referralprogram");
        modelBuilder.Entity<StampRecord>().ToTable("marketing_stamprecord");

        // ── Unique indices ─────────────────────────────────────────────────────
        modelBuilder.Entity<Workspace>().HasIndex(w => w.Slug).IsUnique();
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<Customer>().HasIndex(c => new { c.WorkspaceId, c.UserId }).IsUnique();
        modelBuilder.Entity<CustomerTag>().HasIndex(t => new { t.WorkspaceId, t.Name }).IsUnique();
        modelBuilder.Entity<StaffMember>().HasIndex(m => new { m.WorkspaceId, m.UserId }).IsUnique();
        modelBuilder.Entity<UserPreferences>().HasIndex(p => p.UserId).IsUnique();

        // ── Column name remaps ─────────────────────────────────────────────────
        modelBuilder.Entity<Address>().Property(a => a.WorkspaceId).HasColumnName("workspace_id");
        modelBuilder.Entity<Address>().Property(a => a.ContentTypeId).HasColumnName("content_type_id");
        modelBuilder.Entity<Address>().Property(a => a.ObjectId).HasColumnName("object_id");
        modelBuilder.Entity<CustomerTagCustomer>().Property(x => x.CustomertagId).HasColumnName("customertag_id");
        modelBuilder.Entity<CustomerTagCustomer>().Property(x => x.CustomerId).HasColumnName("customer_id");
    }
}
