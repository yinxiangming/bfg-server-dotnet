using Bfg.Core;

namespace Bfg.Api.Endpoints;

/// <summary>
/// Inbox API under /api/v1/inbox/: messages, templates, recipients, sms.
/// </summary>
public static class InboxEndpoints
{
    public static void MapInboxEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/inbox").WithTags("Inbox").RequireAuthorization();
        group.MapGet("/messages", EmptyList);
        group.MapGet("/templates", EmptyList);
        group.MapGet("/recipients", EmptyList);
        group.MapGet("/sms", EmptyList);
    }

    private static IResult EmptyList() => Results.Ok(Array.Empty<object>());
}
