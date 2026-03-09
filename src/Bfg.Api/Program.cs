using System.Text;
using Bfg.Api.Configuration;
using Bfg.Api.Endpoints;
using Bfg.Api.Middleware;
using Bfg.Api.Services;
using Bfg.Core;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
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
    builder.Services.AddDbContext<BfgDbContext>(o => o.UseNpgsql(conn, b => b.MigrationsAssembly("Bfg.Api")));

builder.Services.AddScoped<JwtService>();

var jwtOpts = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
if (jwtOpts != null && !string.IsNullOrEmpty(jwtOpts.SecretKey))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(o =>
        {
            o.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOpts.SecretKey)),
                ValidIssuer = jwtOpts.Issuer,
                ValidAudience = jwtOpts.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    builder.Services.AddAuthorization();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BFG Framework API", Description = "BFG API compatible with Django DRF (src/server).", Version = "1.0.0" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Name = "Authorization", In = ParameterLocation.Header, Type = SecuritySchemeType.ApiKey });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
});

var app = builder.Build();

app.UseSwagger(c => c.RouteTemplate = "api/schema/{documentName}/swagger.json");
app.UseSwaggerUI(c => { c.SwaggerEndpoint("/api/schema/v1/swagger.json", "BFG API v1"); c.RoutePrefix = "api/docs"; });
app.UseHttpsRedirection();
app.UseMiddleware<WorkspaceMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/v1/", () => Results.Json(new { name = "BFG API", version = "1.0" })).WithTags("Meta");
app.MapAuthEndpoints();
app.MapCommonEndpoints();
app.MapWebEndpoints();
app.MapShopEndpoints();
app.MapStorefrontEndpoints();
app.MapOtherModuleEndpoints();
app.MapInboxEndpoints();

app.Run();
