namespace Bfg.Core.Shop;

/// <summary>
/// M2M join: Store - Warehouse. Django shop.Store.warehouses.
/// </summary>
public class StoreWarehouse
{
    public int StoreId { get; set; }
    public int WarehouseId { get; set; }

    public Store Store { get; set; } = null!;
    public Bfg.Core.Delivery.Warehouse Warehouse { get; set; } = null!;
}
