using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bfg.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_user_common_workspace_default_workspace_id",
                table: "common_user");

            migrationBuilder.DropForeignKey(
                name: "FK_shop_product_categories_shop_productcategory_ProductCategory~",
                table: "shop_product_categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_marketing_voucher",
                table: "marketing_voucher");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "support_supportticket");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "shop_productvariant");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "shop_productvariant");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "shop_productcategory");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "shop_productcategory");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "shop_orderitem");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "shop_cart");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "shop_cart");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "IssuedAt",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                table: "marketing_voucher");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                table: "marketing_voucher");

            migrationBuilder.RenameTable(
                name: "marketing_voucher",
                newName: "marketing_coupon");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "support_ticketmessage",
                newName: "sender_id");

            migrationBuilder.RenameColumn(
                name: "Body",
                table: "support_ticketmessage",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "shop_productvariant",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "shop_productcategory",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "ProductCategoryId",
                table: "shop_product_categories",
                newName: "productcategory_id");

            migrationBuilder.RenameIndex(
                name: "IX_shop_product_categories_ProductCategoryId",
                table: "shop_product_categories",
                newName: "IX_shop_product_categories_productcategory_id");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "shop_orderitem",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "TotalPrice",
                table: "shop_orderitem",
                newName: "subtotal");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "shop_order",
                newName: "total");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "shop_cartitem",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "PaidAt",
                table: "finance_payment",
                newName: "CompletedAt");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "finance_invoice",
                newName: "total");

            migrationBuilder.RenameColumn(
                name: "SortOrder",
                table: "delivery_freightservice",
                newName: "order");

            migrationBuilder.RenameColumn(
                name: "Domain",
                table: "common_workspace",
                newName: "Uuid");

            migrationBuilder.RenameColumn(
                name: "default_workspace_id",
                table: "common_user",
                newName: "DefaultWorkspaceId");

            migrationBuilder.RenameIndex(
                name: "IX_common_user_default_workspace_id",
                table: "common_user",
                newName: "IX_common_user_DefaultWorkspaceId");

            migrationBuilder.RenameColumn(
                name: "UsedCount",
                table: "marketing_coupon",
                newName: "times_used");

            migrationBuilder.AlterColumn<string>(
                name: "Blocks",
                table: "web_page",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "AllowComments",
                table: "web_page",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "web_page",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "web_page",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MetaKeywords",
                table: "web_page",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "web_page",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<short>(
                name: "order",
                table: "web_page",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "web_inquiry",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "is_staff_reply",
                table: "support_ticketmessage",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "support_supportticket",
                keyColumn: "Channel",
                keyValue: null,
                column: "Channel",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "support_supportticket",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "support_supportticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PriorityId",
                table: "support_supportticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketNumber",
                table: "support_supportticket",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "related_order_id",
                table: "support_supportticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Settings",
                table: "shop_store",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Options",
                table: "shop_productvariant",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "shop_productvariant",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "shop_productcategory",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "shop_productcategory",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RuleMatchType",
                table: "shop_productcategory",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Rules",
                table: "shop_productcategory",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Barcode",
                table: "shop_product",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "shop_product",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "shop_product",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FinanceCodeId",
                table: "shop_product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscription",
                table: "shop_product",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LowStockThreshold",
                table: "shop_product",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MetaDescription",
                table: "shop_product",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "MetaTitle",
                table: "shop_product",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresShipping",
                table: "shop_product",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionPlanId",
                table: "shop_product",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "shop_product",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNote",
                table: "shop_order",
                type: "longtext",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "shop_order",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "fulfillment_method",
                table: "shop_order",
                type: "longtext",
                nullable: false,
                defaultValue: "shipping")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "marketing_giftcard",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "marketing_discountrule",
                keyColumn: "DisplayLabel",
                keyValue: null,
                column: "DisplayLabel",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "DisplayLabel",
                table: "marketing_discountrule",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AllocationMethod",
                table: "marketing_discountrule",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "marketing_discountrule",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EntitledProductIds",
                table: "marketing_discountrule",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EntitledQuantity",
                table: "marketing_discountrule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGroupBuy",
                table: "marketing_discountrule",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PrerequisiteProductIds",
                table: "marketing_discountrule",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "PrerequisiteQuantity",
                table: "marketing_discountrule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PromoDisplayOrder",
                table: "marketing_discountrule",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "marketing_campaign",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "marketing_campaign",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "marketing_campaign",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "marketing_campaign",
                keyColumn: "CampaignType",
                keyValue: null,
                column: "CampaignType",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CampaignType",
                table: "marketing_campaign",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "marketing_campaign",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "MaxParticipants",
                table: "marketing_campaign",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinParticipants",
                table: "marketing_campaign",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PromoDisplayType",
                table: "marketing_campaign",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "RequiresParticipation",
                table: "marketing_campaign",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UtmCampaign",
                table: "marketing_campaign",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmMedium",
                table: "marketing_campaign",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "UtmSource",
                table: "marketing_campaign",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AvailableVariables",
                table: "inbox_messagetemplate",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EmailHtmlBody",
                table: "inbox_messagetemplate",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SmsBody",
                table: "inbox_messagetemplate",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ActionLabel",
                table: "inbox_message",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ActionUrl",
                table: "inbox_message",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "inbox_message",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "inbox_message",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedContentTypeId",
                table: "inbox_message",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedObjectId",
                table: "inbox_message",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendEmailAt",
                table: "inbox_message",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendPushAt",
                table: "inbox_message",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SendSmsAt",
                table: "inbox_message",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Config",
                table: "finance_paymentgateway",
                type: "json",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TestConfig",
                table: "finance_paymentgateway",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "WorkspaceId",
                table: "finance_payment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "finance_payment",
                keyColumn: "PaymentNumber",
                keyValue: null,
                column: "PaymentNumber",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentNumber",
                table: "finance_payment",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<long>(
                name: "OrderId",
                table: "finance_payment",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "GatewayId",
                table: "finance_payment",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "CustomerId",
                table: "finance_payment",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CurrencyId",
                table: "finance_payment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "finance_payment",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "GatewayDisplayName",
                table: "finance_payment",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GatewayResponse",
                table: "finance_payment",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GatewayTransactionId",
                table: "finance_payment",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GatewayType",
                table: "finance_payment",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "InvoiceId",
                table: "finance_payment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PaymentMethodId",
                table: "finance_payment",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "finance_invoice",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "finance_invoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "finance_invoice",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "finance_invoice",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Tax",
                table: "finance_invoice",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "due_date",
                table: "finance_invoice",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "issue_date",
                table: "finance_invoice",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "paid_date",
                table: "finance_invoice",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "delivery_warehouse",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "delivery_warehouse",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "delivery_warehouse",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "delivery_warehouse",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "delivery_freightservice",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "delivery_freightservice",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDaysMax",
                table: "delivery_freightservice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDaysMin",
                table: "delivery_freightservice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxWeight",
                table: "delivery_freightservice",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinWeight",
                table: "delivery_freightservice",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TransportType",
                table: "delivery_freightservice",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Config",
                table: "delivery_carrier",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "IsTestMode",
                table: "delivery_carrier",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TestConfig",
                table: "delivery_carrier",
                type: "json",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "TrackingUrlTemplate",
                table: "delivery_carrier",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValidFrom",
                table: "marketing_coupon",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DiscountRuleId",
                table: "marketing_coupon",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "marketing_coupon",
                keyColumn: "Description",
                keyValue: null,
                column: "Description",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "marketing_coupon",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "BoostId",
                table: "marketing_coupon",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_marketing_coupon",
                table: "marketing_coupon",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "common_apikey",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyPrefix = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Prefix = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    KeyHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Permissions = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastUsedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_apikey", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_consignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    ConsignmentNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackingNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShipDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EstimatedDelivery = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ActualDelivery = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RecipientAddressId = table.Column<int>(type: "int", nullable: false),
                    SenderAddressId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ManifestId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_consignment", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_consignment_orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConsignmentId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_consignment_orders", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_freightstatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SendMessage = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MappedConsignmentStatusId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_freightstatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_package",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    PackageNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TrackingNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Weight = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Length = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Width = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Height = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Pieces = table.Column<int>(type: "int", nullable: true),
                    State = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ConsignmentId = table.Column<int>(type: "int", nullable: true),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    StorageLocationId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_package", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_packagetemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Length = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Width = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Height = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TareWeight = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MaxWeight = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_packagetemplate", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_packagingtype",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_packagingtype", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "delivery_trackingevent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    ContentTypeId = table.Column<int>(type: "int", nullable: true),
                    ObjectId = table.Column<int>(type: "int", nullable: true),
                    ConsignmentId = table.Column<int>(type: "int", nullable: true),
                    PackageId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPublic = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EventTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OccurredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_trackingevent", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "django_content_type",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AppLabel = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Model = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_brand",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Logo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AddressId = table.Column<int>(type: "int", nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TaxId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegistrationNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InvoiceNote = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_brand", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_financialcode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Unit = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaxType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_financialcode", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_invoiceitem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TaxType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FinancialCodeId = table.Column<int>(type: "int", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_invoiceitem", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_taxrate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Country = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    State = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_taxrate", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_transaction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    TransactionType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: true),
                    WalletId = table.Column<int>(type: "int", nullable: true),
                    BalanceType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TxStatus = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SourceId = table.Column<int>(type: "int", nullable: true),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_transaction", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_wallet",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CashBalance = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CreditBalance = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CurrencyId = table.Column<int>(type: "int", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_wallet", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "finance_withdrawalrequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    TransactionId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PayoutMethod = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PayoutDetails = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RejectionReason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RequestedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedById = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_finance_withdrawalrequest", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inbox_messagerecipient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: false),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsArchived = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox_messagerecipient", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marketing_campaigndisplay",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DisplayType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtitle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Image = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LinkTarget = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rules = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CampaignId = table.Column<int>(type: "int", nullable: true),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    WorkspaceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketing_campaigndisplay", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marketing_campaignparticipation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StampCount = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RewardClaimedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketing_campaignparticipation", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marketing_referralprogram",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RewardType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RewardValue = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MinPurchase = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketing_referralprogram", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marketing_stamprecord",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CampaignId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    StampCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketing_stamprecord", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_collection",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Image = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_collection", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_collection_products",
                columns: table => new
                {
                    CollectionId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_collection_products", x => new { x.CollectionId, x.ProductId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_product_tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    producttag_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_product_tags", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_productchannellisting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    AvailableAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_productchannellisting", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_productreview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Comment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Images = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsVerifiedPurchase = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HelpfulCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_productreview", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_producttag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_producttag", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_return",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    ReturnNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_return", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_returnitem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReturnId = table.Column<int>(type: "int", nullable: false),
                    OrderItemId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Condition = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_returnitem", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_saleschannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Code = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChannelType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Config = table.Column<string>(type: "json", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_saleschannel", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_wishlist",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    SessionKey = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_wishlist", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shop_wishlistitem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WishlistId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shop_wishlistitem", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "support_ticketcategory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_ticketcategory", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "support_ticketpriority",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseTimeHours = table.Column<int>(type: "int", nullable: false),
                    ResolutionTimeHours = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_ticketpriority", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_booking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    TimeslotId = table.Column<int>(type: "int", nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AdminNotes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_booking", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_bookingtimeslot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    SlotType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    MaxBookings = table.Column<int>(type: "int", nullable: false),
                    CurrentBookings = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_bookingtimeslot", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    ContentTypeName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Color = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_category", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_menu",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Location = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_menu", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_menuitem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MenuId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Label = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Url = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PageId = table.Column<int>(type: "int", nullable: true),
                    PostId = table.Column<int>(type: "int", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    Icon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CssClass = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    order = table.Column<int>(type: "int", nullable: false),
                    OpenInNewTab = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_menuitem", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_newslettersend",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TemplateId = table.Column<int>(type: "int", nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_newslettersend", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_newslettersubscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    SiteId = table.Column<int>(type: "int", nullable: true),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SourceUrl = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IpAddress = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnsubscribeToken = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_newslettersubscription", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_newslettertemplate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subject = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SubjectTemplate = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Body = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BodyHtml = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_newslettertemplate", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_post",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Excerpt = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FeaturedImage = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    MetaTitle = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MetaDescription = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublishedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    AllowComments = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AuthorId = table.Column<int>(type: "int", nullable: true),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_post", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "web_tag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkspaceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Slug = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_web_tag", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_shop_productchannellisting_ProductId_ChannelId",
                table: "shop_productchannellisting",
                columns: new[] { "ProductId", "ChannelId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_common_user_common_workspace_DefaultWorkspaceId",
                table: "common_user",
                column: "DefaultWorkspaceId",
                principalTable: "common_workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_shop_product_categories_shop_productcategory_productcategory~",
                table: "shop_product_categories",
                column: "productcategory_id",
                principalTable: "shop_productcategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_common_user_common_workspace_DefaultWorkspaceId",
                table: "common_user");

            migrationBuilder.DropForeignKey(
                name: "FK_shop_product_categories_shop_productcategory_productcategory~",
                table: "shop_product_categories");

            migrationBuilder.DropTable(
                name: "common_apikey");

            migrationBuilder.DropTable(
                name: "delivery_consignment");

            migrationBuilder.DropTable(
                name: "delivery_consignment_orders");

            migrationBuilder.DropTable(
                name: "delivery_freightstatus");

            migrationBuilder.DropTable(
                name: "delivery_package");

            migrationBuilder.DropTable(
                name: "delivery_packagetemplate");

            migrationBuilder.DropTable(
                name: "delivery_packagingtype");

            migrationBuilder.DropTable(
                name: "delivery_trackingevent");

            migrationBuilder.DropTable(
                name: "django_content_type");

            migrationBuilder.DropTable(
                name: "finance_brand");

            migrationBuilder.DropTable(
                name: "finance_financialcode");

            migrationBuilder.DropTable(
                name: "finance_invoiceitem");

            migrationBuilder.DropTable(
                name: "finance_taxrate");

            migrationBuilder.DropTable(
                name: "finance_transaction");

            migrationBuilder.DropTable(
                name: "finance_wallet");

            migrationBuilder.DropTable(
                name: "finance_withdrawalrequest");

            migrationBuilder.DropTable(
                name: "inbox_messagerecipient");

            migrationBuilder.DropTable(
                name: "marketing_campaigndisplay");

            migrationBuilder.DropTable(
                name: "marketing_campaignparticipation");

            migrationBuilder.DropTable(
                name: "marketing_referralprogram");

            migrationBuilder.DropTable(
                name: "marketing_stamprecord");

            migrationBuilder.DropTable(
                name: "shop_collection");

            migrationBuilder.DropTable(
                name: "shop_collection_products");

            migrationBuilder.DropTable(
                name: "shop_product_tags");

            migrationBuilder.DropTable(
                name: "shop_productchannellisting");

            migrationBuilder.DropTable(
                name: "shop_productreview");

            migrationBuilder.DropTable(
                name: "shop_producttag");

            migrationBuilder.DropTable(
                name: "shop_return");

            migrationBuilder.DropTable(
                name: "shop_returnitem");

            migrationBuilder.DropTable(
                name: "shop_saleschannel");

            migrationBuilder.DropTable(
                name: "shop_wishlist");

            migrationBuilder.DropTable(
                name: "shop_wishlistitem");

            migrationBuilder.DropTable(
                name: "support_ticketcategory");

            migrationBuilder.DropTable(
                name: "support_ticketpriority");

            migrationBuilder.DropTable(
                name: "web_booking");

            migrationBuilder.DropTable(
                name: "web_bookingtimeslot");

            migrationBuilder.DropTable(
                name: "web_category");

            migrationBuilder.DropTable(
                name: "web_menu");

            migrationBuilder.DropTable(
                name: "web_menuitem");

            migrationBuilder.DropTable(
                name: "web_newslettersend");

            migrationBuilder.DropTable(
                name: "web_newslettersubscription");

            migrationBuilder.DropTable(
                name: "web_newslettertemplate");

            migrationBuilder.DropTable(
                name: "web_post");

            migrationBuilder.DropTable(
                name: "web_tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_marketing_coupon",
                table: "marketing_coupon");

            migrationBuilder.DropColumn(
                name: "AllowComments",
                table: "web_page");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "web_page");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "web_page");

            migrationBuilder.DropColumn(
                name: "MetaKeywords",
                table: "web_page");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "web_page");

            migrationBuilder.DropColumn(
                name: "order",
                table: "web_page");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "web_inquiry");

            migrationBuilder.DropColumn(
                name: "is_staff_reply",
                table: "support_ticketmessage");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "support_supportticket");

            migrationBuilder.DropColumn(
                name: "PriorityId",
                table: "support_supportticket");

            migrationBuilder.DropColumn(
                name: "TicketNumber",
                table: "support_supportticket");

            migrationBuilder.DropColumn(
                name: "related_order_id",
                table: "support_supportticket");

            migrationBuilder.DropColumn(
                name: "Settings",
                table: "shop_store");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "shop_productvariant");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "shop_productcategory");

            migrationBuilder.DropColumn(
                name: "Image",
                table: "shop_productcategory");

            migrationBuilder.DropColumn(
                name: "RuleMatchType",
                table: "shop_productcategory");

            migrationBuilder.DropColumn(
                name: "Rules",
                table: "shop_productcategory");

            migrationBuilder.DropColumn(
                name: "Barcode",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "FinanceCodeId",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "IsSubscription",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "LowStockThreshold",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "MetaDescription",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "MetaTitle",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "RequiresShipping",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "shop_product");

            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "shop_order");

            migrationBuilder.DropColumn(
                name: "fulfillment_method",
                table: "shop_order");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "marketing_giftcard");

            migrationBuilder.DropColumn(
                name: "AllocationMethod",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "Config",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "EntitledProductIds",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "EntitledQuantity",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "IsGroupBuy",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "PrerequisiteProductIds",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "PrerequisiteQuantity",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "PromoDisplayOrder",
                table: "marketing_discountrule");

            migrationBuilder.DropColumn(
                name: "Config",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "MaxParticipants",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "MinParticipants",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "PromoDisplayType",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "RequiresParticipation",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "UtmCampaign",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "UtmMedium",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "UtmSource",
                table: "marketing_campaign");

            migrationBuilder.DropColumn(
                name: "AvailableVariables",
                table: "inbox_messagetemplate");

            migrationBuilder.DropColumn(
                name: "EmailHtmlBody",
                table: "inbox_messagetemplate");

            migrationBuilder.DropColumn(
                name: "SmsBody",
                table: "inbox_messagetemplate");

            migrationBuilder.DropColumn(
                name: "ActionLabel",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "ActionUrl",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "RelatedContentTypeId",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "RelatedObjectId",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "SendEmailAt",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "SendPushAt",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "SendSmsAt",
                table: "inbox_message");

            migrationBuilder.DropColumn(
                name: "TestConfig",
                table: "finance_paymentgateway");

            migrationBuilder.DropColumn(
                name: "GatewayDisplayName",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "GatewayResponse",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "GatewayTransactionId",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "GatewayType",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "finance_payment");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "Tax",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "due_date",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "issue_date",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "paid_date",
                table: "finance_invoice");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "delivery_warehouse");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "delivery_warehouse");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "delivery_warehouse");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "delivery_warehouse");

            migrationBuilder.DropColumn(
                name: "Config",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "EstimatedDaysMax",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "EstimatedDaysMin",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "MaxWeight",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "MinWeight",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "TransportType",
                table: "delivery_freightservice");

            migrationBuilder.DropColumn(
                name: "Config",
                table: "delivery_carrier");

            migrationBuilder.DropColumn(
                name: "IsTestMode",
                table: "delivery_carrier");

            migrationBuilder.DropColumn(
                name: "TestConfig",
                table: "delivery_carrier");

            migrationBuilder.DropColumn(
                name: "TrackingUrlTemplate",
                table: "delivery_carrier");

            migrationBuilder.DropColumn(
                name: "BoostId",
                table: "marketing_coupon");

            migrationBuilder.RenameTable(
                name: "marketing_coupon",
                newName: "marketing_voucher");

            migrationBuilder.RenameColumn(
                name: "sender_id",
                table: "support_ticketmessage",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "support_ticketmessage",
                newName: "Body");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "shop_productvariant",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "shop_productcategory",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "productcategory_id",
                table: "shop_product_categories",
                newName: "ProductCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_shop_product_categories_productcategory_id",
                table: "shop_product_categories",
                newName: "IX_shop_product_categories_ProductCategoryId");

            migrationBuilder.RenameColumn(
                name: "subtotal",
                table: "shop_orderitem",
                newName: "TotalPrice");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "shop_orderitem",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "total",
                table: "shop_order",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "shop_cartitem",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "finance_payment",
                newName: "PaidAt");

            migrationBuilder.RenameColumn(
                name: "total",
                table: "finance_invoice",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "order",
                table: "delivery_freightservice",
                newName: "SortOrder");

            migrationBuilder.RenameColumn(
                name: "Uuid",
                table: "common_workspace",
                newName: "Domain");

            migrationBuilder.RenameColumn(
                name: "DefaultWorkspaceId",
                table: "common_user",
                newName: "default_workspace_id");

            migrationBuilder.RenameIndex(
                name: "IX_common_user_DefaultWorkspaceId",
                table: "common_user",
                newName: "IX_common_user_default_workspace_id");

            migrationBuilder.RenameColumn(
                name: "times_used",
                table: "marketing_voucher",
                newName: "UsedCount");

            migrationBuilder.AlterColumn<string>(
                name: "Blocks",
                table: "web_page",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Channel",
                table: "support_supportticket",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "support_supportticket",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Options",
                table: "shop_productvariant",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "shop_productvariant",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "shop_productvariant",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "shop_productcategory",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "shop_productcategory",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "shop_orderitem",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "CustomerNote",
                table: "shop_order",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldDefaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "shop_cart",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "StoreId",
                table: "shop_cart",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayLabel",
                table: "marketing_discountrule",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "marketing_discountrule",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartDate",
                table: "marketing_campaign",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "marketing_campaign",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CampaignType",
                table: "marketing_campaign",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Config",
                table: "finance_paymentgateway",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "json")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "WorkspaceId",
                table: "finance_payment",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentNumber",
                table: "finance_payment",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "finance_payment",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "GatewayId",
                table: "finance_payment",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "finance_payment",
                type: "int",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyId",
                table: "finance_payment",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "finance_payment",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<string>(
                name: "TransactionId",
                table: "finance_payment",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "CustomerId",
                table: "finance_invoice",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "IssuedAt",
                table: "finance_invoice",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "delivery_freightservice",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "delivery_freightservice",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "ValidFrom",
                table: "marketing_voucher",
                type: "datetime(6)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<int>(
                name: "DiscountRuleId",
                table: "marketing_voucher",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "marketing_voucher",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                table: "marketing_voucher",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                table: "marketing_voucher",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_marketing_voucher",
                table: "marketing_voucher",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_common_user_common_workspace_default_workspace_id",
                table: "common_user",
                column: "default_workspace_id",
                principalTable: "common_workspace",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_shop_product_categories_shop_productcategory_ProductCategory~",
                table: "shop_product_categories",
                column: "ProductCategoryId",
                principalTable: "shop_productcategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
