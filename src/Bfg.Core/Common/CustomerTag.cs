namespace Bfg.Core.Common;

/// <summary>
/// Customer tag. Matches Django common.CustomerTag. M2M with Customer via CustomerTagCustomer.
/// </summary>
public class CustomerTag
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public ICollection<CustomerTagCustomer> CustomerTagCustomers { get; set; } = new List<CustomerTagCustomer>();
}

/// <summary>
/// M2M join table: common_customertag_customers in Django.
/// </summary>
public class CustomerTagCustomer
{
    public int Id { get; set; }
    public int CustomertagId { get; set; }
    public int CustomerId { get; set; }

    public CustomerTag CustomerTag { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
}
