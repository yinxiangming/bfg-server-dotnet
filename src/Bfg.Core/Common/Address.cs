namespace Bfg.Core.Common;

/// <summary>
/// Generic address. Matches Django common.Address (GenericForeignKey via content_type_id, object_id).
/// </summary>
public class Address
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? ContentTypeId { get; set; }
    public int? ObjectId { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Company { get; set; } = "";
    public string AddressLine1 { get; set; } = "";
    public string AddressLine2 { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";
    public string Country { get; set; } = "";
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string Notes { get; set; } = "";
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
}
