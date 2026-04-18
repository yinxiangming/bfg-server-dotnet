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

        // Delivery: only keep placeholders for routes not implemented elsewhere.
        api.MapGet("/packaging-types", EmptyList);
        api.MapGet("/tracking-events", EmptyList);
        api.MapGet("/packages", EmptyList);

        // Marketing: keep only placeholders for routes not implemented elsewhere.
        api.MapGet("/campaign-participations", EmptyList);
        api.MapGet("/stamp-records", EmptyList);
        api.MapGet("/referral-programs", EmptyList);

        // Support: tickets
        api.MapGet("/tickets", EmptyList);

        // Finance: keep only placeholder routes not implemented elsewhere.
        api.MapGet("/payment-methods", EmptyList);
        api.MapGet("/brands", EmptyList);
        api.MapGet("/financial-codes", EmptyList);
        api.MapGet("/tax-rates", EmptyList);
    }

    private static IResult EmptyList() => Results.Ok(Array.Empty<object>());
}
