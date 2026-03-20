using Bfg.Core.Shop;

namespace Bfg.Api.Services;

public sealed record OrderCheckoutLineDto(int Id, int Product, int? Variant, int Quantity, string UnitPrice, string TotalPrice);

public sealed record OrderCheckoutPayload(Order Order, IReadOnlyList<OrderCheckoutLineDto> Items);

public sealed record OrderCheckoutResult(bool Success, string? ErrorCode, string? ErrorMessage, OrderCheckoutPayload? Payload)
{
    public static OrderCheckoutResult Ok(OrderCheckoutPayload p) => new(true, null, null, p);

    public static OrderCheckoutResult Fail(string code, string message) => new(false, code, message, null);
}
