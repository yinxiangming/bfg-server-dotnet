using Bfg.Core;

namespace Bfg.Api.Endpoints;

/// <summary>
/// Stub routes for Delivery, Marketing, Support, Inbox, Finance so API paths match Django.
/// Returns empty lists or placeholder; entities can be added and wired later.
/// </summary>
public static class OtherModuleEndpoints
{
    public static void MapOtherModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1").WithTags("Other").RequireAuthorization();

        // Delivery: warehouses, consignments, carriers, freight-services, packaging-types, freight-statuses, tracking-events, delivery-zones, packages, package-templates
        api.MapGet("/warehouses", EmptyList);
        api.MapGet("/consignments", EmptyList);
        api.MapGet("/carriers", EmptyList);
        api.MapGet("/freight-services", EmptyList);
        api.MapGet("/packaging-types", EmptyList);
        api.MapGet("/freight-statuses", EmptyList);
        api.MapGet("/tracking-events", EmptyList);
        api.MapGet("/delivery-zones", EmptyList);
        api.MapGet("/packages", EmptyList);
        api.MapGet("/package-templates", EmptyList);

        // Marketing: campaigns, campaign-displays, campaign-participations, stamp-records, coupons, gift-cards, referral-programs, discount-rules
        api.MapGet("/campaigns", EmptyList);
        api.MapGet("/campaign-displays", EmptyList);
        api.MapGet("/campaign-participations", EmptyList);
        api.MapGet("/stamp-records", EmptyList);
        api.MapGet("/coupons", EmptyList);
        api.MapGet("/gift-cards", EmptyList);
        api.MapGet("/referral-programs", EmptyList);
        api.MapGet("/discount-rules", EmptyList);

        // Support: tickets
        api.MapGet("/tickets", EmptyList);

        // Finance: invoices, payments, payment-methods, payment-gateways, currencies, brands, financial-codes, tax-rates
        api.MapGet("/invoices", EmptyList);
        api.MapGet("/payments", EmptyList);
        api.MapGet("/payment-methods", EmptyList);
        api.MapGet("/payment-gateways", EmptyList);
        api.MapGet("/currencies", EmptyList);
        api.MapGet("/brands", EmptyList);
        api.MapGet("/financial-codes", EmptyList);
        api.MapGet("/tax-rates", EmptyList);
    }

    private static IResult EmptyList() => Results.Ok(Array.Empty<object>());
}
