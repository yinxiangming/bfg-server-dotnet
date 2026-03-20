namespace Bfg.Api.Services;

/// <summary>
/// Maps cart DTOs to JSON-serializable shapes for API responses.
/// </summary>
public static class CartJson
{
    public static object Detail(CartDetail d) => new
    {
        id = d.Id,
        workspace = d.Workspace,
        customer = d.Customer,
        status = d.Status,
        total = d.Total,
        items = LineItems(d.Items)
    };

    /// <summary>Storefront omits redundant total duplicate in some clients; same shape as <see cref="Detail"/>.</summary>
    public static object StorefrontDetail(CartDetail d) => new
    {
        id = d.Id,
        workspace = d.Workspace,
        customer = d.Customer,
        status = d.Status,
        items = StorefrontLineItems(d.Items),
        total = d.Total
    };

    public static object ListRow(CartListRow r) => new { id = r.Id, workspace = r.Workspace, customer = r.Customer, status = r.Status };

    public static object ClearedCart(CartDetail d) => new
    {
        id = d.Id,
        workspace = d.Workspace,
        customer = d.Customer,
        status = d.Status,
        items = Array.Empty<object>(),
        total = d.Total
    };

    private static IReadOnlyList<object> LineItems(IReadOnlyList<CartLineDto> items) =>
        items.Select(i => (object)new
        {
            id = i.Id,
            product = i.Product,
            variant = i.Variant,
            quantity = i.Quantity,
            unit_price = i.UnitPrice,
            total_price = i.TotalPrice,
            variant_options = i.VariantOptions
        }).ToList();

    private static IReadOnlyList<object> StorefrontLineItems(IReadOnlyList<CartLineDto> items) =>
        items.Select(i => (object)new
        {
            id = i.Id,
            item_id = i.Id,
            product = i.Product,
            variant = i.Variant,
            quantity = i.Quantity,
            unit_price = i.UnitPrice,
            total_price = i.TotalPrice,
            image_url = "",
            variant_options = i.VariantOptions
        }).ToList();
}
