using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Bfg.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCommon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "common_auditlog",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ContentTypeId = table.Column<int>(type: "integer", nullable: true),
                    ObjectId = table.Column<int>(type: "integer", nullable: true),
                    ObjectRepr = table.Column<string>(type: "text", nullable: false),
                    Changes = table.Column<string>(type: "text", nullable: false),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_auditlog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "common_workspace",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Settings = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_workspace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "common_address",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    workspace_id = table.Column<int>(type: "integer", nullable: false),
                    content_type_id = table.Column<int>(type: "integer", nullable: true),
                    object_id = table.Column<int>(type: "integer", nullable: true),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false),
                    AddressLine1 = table.Column<string>(type: "text", nullable: false),
                    AddressLine2 = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    PostalCode = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_address_common_workspace_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_customersegment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Query = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_customersegment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_customersegment_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_customertag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_customertag", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_customertag_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_emailconfig",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    BackendType = table.Column<string>(type: "text", nullable: false),
                    Config = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_emailconfig", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_emailconfig_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    SiteName = table.Column<string>(type: "text", nullable: false),
                    SiteDescription = table.Column<string>(type: "text", nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: false),
                    Favicon = table.Column<string>(type: "text", nullable: false),
                    DefaultLanguage = table.Column<string>(type: "text", nullable: false),
                    SupportedLanguages = table.Column<string>(type: "text", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "text", nullable: false),
                    DefaultTimezone = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    SupportEmail = table.Column<string>(type: "text", nullable: false),
                    ContactPhone = table.Column<string>(type: "text", nullable: false),
                    FacebookUrl = table.Column<string>(type: "text", nullable: false),
                    TwitterUrl = table.Column<string>(type: "text", nullable: false),
                    InstagramUrl = table.Column<string>(type: "text", nullable: false),
                    Features = table.Column<string>(type: "text", nullable: false),
                    CustomSettings = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_settings_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_staffrole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Permissions = table.Column<string>(type: "text", nullable: false),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_staffrole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_staffrole_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_user",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Password = table.Column<string>(type: "text", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSuperuser = table.Column<bool>(type: "boolean", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    IsStaff = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DateJoined = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    Avatar = table.Column<string>(type: "text", nullable: false),
                    default_workspace_id = table.Column<int>(type: "integer", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: false),
                    TimezoneName = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_user", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_user_common_workspace_default_workspace_id",
                        column: x => x.default_workspace_id,
                        principalTable: "common_workspace",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "common_customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CustomerNumber = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: false),
                    TaxNumber = table.Column<string>(type: "text", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "numeric", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    GatewayMetadata = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_customer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_customer_common_user_UserId",
                        column: x => x.UserId,
                        principalTable: "common_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_customer_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_media",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    File = table.Column<string>(type: "text", nullable: true),
                    ExternalUrl = table.Column<string>(type: "text", nullable: false),
                    MediaType = table.Column<string>(type: "text", nullable: false),
                    AltText = table.Column<string>(type: "text", nullable: false),
                    Width = table.Column<int>(type: "integer", nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    UploadedById = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_media", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_media_common_user_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "common_user",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_common_media_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_staffmember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_staffmember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_staffmember_common_staffrole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "common_staffrole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_staffmember_common_user_UserId",
                        column: x => x.UserId,
                        principalTable: "common_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_staffmember_common_workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "common_workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_userpreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    EmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    SmsNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    PushNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOrderUpdates = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyPromotions = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyProductUpdates = table.Column<bool>(type: "boolean", nullable: false),
                    NotifySupportReplies = table.Column<bool>(type: "boolean", nullable: false),
                    ProfileVisibility = table.Column<string>(type: "text", nullable: false),
                    ShowEmail = table.Column<bool>(type: "boolean", nullable: false),
                    ShowPhone = table.Column<bool>(type: "boolean", nullable: false),
                    Theme = table.Column<string>(type: "text", nullable: false),
                    ItemsPerPage = table.Column<int>(type: "integer", nullable: false),
                    CustomPreferences = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_userpreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_userpreferences_common_user_UserId",
                        column: x => x.UserId,
                        principalTable: "common_user",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_customertag_customers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    customertag_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_customertag_customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_customertag_customers_common_customer_customer_id",
                        column: x => x.customer_id,
                        principalTable: "common_customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_customertag_customers_common_customertag_customertag~",
                        column: x => x.customertag_id,
                        principalTable: "common_customertag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_medialink",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MediaId = table.Column<int>(type: "integer", nullable: false),
                    ContentTypeId = table.Column<int>(type: "integer", nullable: false),
                    ObjectId = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_medialink", x => x.Id);
                    table.ForeignKey(
                        name: "FK_common_medialink_common_media_MediaId",
                        column: x => x.MediaId,
                        principalTable: "common_media",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_common_address_workspace_id",
                table: "common_address",
                column: "workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_customer_UserId",
                table: "common_customer",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_common_customer_WorkspaceId_UserId",
                table: "common_customer",
                columns: new[] { "WorkspaceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_customersegment_WorkspaceId",
                table: "common_customersegment",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_common_customertag_WorkspaceId_Name",
                table: "common_customertag",
                columns: new[] { "WorkspaceId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_customertag_customers_customer_id",
                table: "common_customertag_customers",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_customertag_customers_customertag_id",
                table: "common_customertag_customers",
                column: "customertag_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_emailconfig_WorkspaceId",
                table: "common_emailconfig",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_common_media_UploadedById",
                table: "common_media",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_common_media_WorkspaceId",
                table: "common_media",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_common_medialink_MediaId",
                table: "common_medialink",
                column: "MediaId");

            migrationBuilder.CreateIndex(
                name: "IX_common_settings_WorkspaceId",
                table: "common_settings",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_common_staffmember_RoleId",
                table: "common_staffmember",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_common_staffmember_UserId",
                table: "common_staffmember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_common_staffmember_WorkspaceId_UserId",
                table: "common_staffmember",
                columns: new[] { "WorkspaceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_staffrole_WorkspaceId",
                table: "common_staffrole",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_common_user_default_workspace_id",
                table: "common_user",
                column: "default_workspace_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_user_Username",
                table: "common_user",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_userpreferences_UserId",
                table: "common_userpreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_workspace_Slug",
                table: "common_workspace",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "common_address");

            migrationBuilder.DropTable(
                name: "common_auditlog");

            migrationBuilder.DropTable(
                name: "common_customersegment");

            migrationBuilder.DropTable(
                name: "common_customertag_customers");

            migrationBuilder.DropTable(
                name: "common_emailconfig");

            migrationBuilder.DropTable(
                name: "common_medialink");

            migrationBuilder.DropTable(
                name: "common_settings");

            migrationBuilder.DropTable(
                name: "common_staffmember");

            migrationBuilder.DropTable(
                name: "common_userpreferences");

            migrationBuilder.DropTable(
                name: "common_customer");

            migrationBuilder.DropTable(
                name: "common_customertag");

            migrationBuilder.DropTable(
                name: "common_media");

            migrationBuilder.DropTable(
                name: "common_staffrole");

            migrationBuilder.DropTable(
                name: "common_user");

            migrationBuilder.DropTable(
                name: "common_workspace");
        }
    }
}
