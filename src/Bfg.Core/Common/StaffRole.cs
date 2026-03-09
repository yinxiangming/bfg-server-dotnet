namespace Bfg.Core.Common;

/// <summary>
/// Staff role. Matches Django common.StaffRole.
/// </summary>
public class StaffRole
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public string Permissions { get; set; } = "{}";
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
}
