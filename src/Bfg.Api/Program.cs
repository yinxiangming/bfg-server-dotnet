using System.Text;
using Bfg.Api.Configuration;
using Bfg.Api.Endpoints;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

// Load .env from project root or current dir (like Django's load_dotenv); env vars override .env
Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AppOptions>(opts =>
{
    opts.FrontendUrl = builder.Configuration.GetFrontendUrl();
    opts.SiteName = builder.Configuration.GetSiteName();
});
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

var conn = builder.Configuration.GetDatabaseConnectionString();
if (!string.IsNullOrEmpty(conn))
    builder.Services.AddDbContext<BfgDbContext>(o => o.UseMySql(conn, ServerVersion.Parse("8.0.21"), b => b.MigrationsAssembly("Bfg.Api")));

builder.Services.AddScoped<JwtService>();

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
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BFG Framework API", Description = "BFG API compatible with Django DRF (src/server).", Version = "1.0.0" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
    // Resolve duplicate method+path (e.g. GET /api/v1/me vs GET /api/v1/me/) due to trailing-slash normalization
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

var app = builder.Build();

app.UseSwagger(c => c.RouteTemplate = "api/schema/{documentName}/swagger.json");
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/api/schema/v1/swagger.json", "BFG API v1"); c.RoutePrefix = "api/docs"; });
app.UseHttpsRedirection();

// Handle Django-style trailing slashes: redirect /foo/ → /foo
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value;
    if (path != null && path.Length > 1 && path.EndsWith("/"))
    {
        ctx.Request.Path = path.TrimEnd('/');
    }
    await next();
});

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
app.MapOtherModuleEndpoints();
app.MapInboxEndpoints();

app.Run();
