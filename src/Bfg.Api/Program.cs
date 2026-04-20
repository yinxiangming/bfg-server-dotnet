using System.Text;
using System.Text.Json;
using Bfg.Api.Configuration;
using Bfg.Api.Endpoints;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using DotNetEnv;
using EFCore.NamingConventions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

// Load .env without overwriting vars already set (shell/CI DATABASE_URL must win over repo .env).
Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
    o.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    // E2E clients assert presence of false/null fields (e.g. is_default, rating).
    o.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

builder.Services.Configure<AppOptions>(opts =>
{
    opts.FrontendUrl = builder.Configuration.GetFrontendUrl();
    opts.SiteName = builder.Configuration.GetSiteName();
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var conn = builder.Configuration.GetDatabaseConnectionString();
if (!string.IsNullOrEmpty(conn))
    builder.Services.AddDbContext<BfgDbContext>(o =>
        o.UseMySql(conn, ServerVersion.Parse("8.0.21"), b => b.MigrationsAssembly("Bfg.Api"))
            .UseSnakeCaseNamingConvention());

builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<OrderCheckoutService>();

var jwtOpts = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
var secretKey = jwtOpts?.SecretKey;
if (string.IsNullOrWhiteSpace(secretKey))
    secretKey = "bfg-dev-fallback-secret-key-32-chars!";
if (secretKey.Length < 32) secretKey = secretKey.PadRight(32, '!');
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidIssuer = jwtOpts?.Issuer ?? "BFG",
            ValidAudience = jwtOpts?.Audience ?? "BFG",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BFG Framework API", Description = "BFG server API (routes and payloads aligned with the canonical BFG implementation).", Version = "1.0.0" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    // Resolve duplicate method+path (e.g. GET /api/v1/me vs GET /api/v1/me/) due to trailing-slash normalization
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var app = builder.Build();

app.UseSwagger(c => c.RouteTemplate = "api/schema/{documentName}/swagger.json");
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/api/schema/v1/swagger.json", "BFG API v1"); c.RoutePrefix = "api/docs"; });
// E2E and local dev hit HTTP only (e.g. :3002); avoid redirecting to HTTPS.
// Do not strip trailing slashes: routes are registered with Django-style /.../ paths.

app.UseMiddleware<WorkspaceMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/v1/", () => Results.Json(new { name = "BFG API", version = "1.0" })).WithTags("Meta");
app.MapAuthEndpoints();
app.MapCommonEndpoints();
app.MapWebEndpoints();
app.MapShopEndpoints();
app.MapDeliveryEndpoints();
app.MapFinanceEndpoints();
app.MapSupportEndpoints();
app.MapMarketingEndpoints();
app.MapStorefrontEndpoints();
app.MapMeEndpoints();
app.MapPlatformEndpoints();
app.MapOtherModuleEndpoints();
app.MapInboxEndpoints();

app.Run();
