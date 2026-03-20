namespace Bfg.Api.Services;

public static class OrderCheckoutJson
{
    public static object CreatedBody(OrderCheckoutPayload p)
    {
        var o = p.Order;
        return new
        {
            id = o.Id,
            order_number = o.OrderNumber,
            workspace = o.WorkspaceId,
            customer_id = o.CustomerId,
            store_id = o.StoreId,
            status = o.Status,
            payment_status = o.PaymentStatus,
            subtotal = o.Subtotal.ToString("F2"),
            shipping_cost = o.ShippingCost.ToString("F2"),
            tax = o.Tax.ToString("F2"),
            discount = o.Discount.ToString("F2"),
            total = o.TotalAmount.ToString("F2"),
            subtotal_amount = o.Subtotal.ToString("F2"),
            shipping_amount = o.ShippingCost.ToString("F2"),
            discount_amount = o.Discount.ToString("F2"),
            total_amount = o.TotalAmount.ToString("F2"),
            amounts = new
            {
                subtotal = o.Subtotal.ToString("F2"),
                shipping_cost = o.ShippingCost.ToString("F2"),
                tax = o.Tax.ToString("F2"),
                discount = o.Discount.ToString("F2"),
                total = o.TotalAmount.ToString("F2")
            },
            shipping_address_id = o.ShippingAddressId,
            billing_address_id = o.BillingAddressId,
            items = p.Items.Select(i => (object)new
            {
                id = i.Id,
                product = i.Product,
                variant = i.Variant,
                quantity = i.Quantity,
                unit_price = i.UnitPrice,
                total_price = i.TotalPrice
            }).ToList(),
            created_at = o.CreatedAt,
            updated_at = o.UpdatedAt
        };
    }
}
