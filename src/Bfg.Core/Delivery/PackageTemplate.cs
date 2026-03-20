namespace Bfg.Core.Delivery;

/// <summary>
/// Reusable package dimensions. Maps to delivery_packagetemplate.
/// </summary>
public class PackageTemplate
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public decimal TareWeight { get; set; }
    public decimal? MaxWeight { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
