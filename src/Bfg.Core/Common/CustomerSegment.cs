namespace Bfg.Core.Common;

/// <summary>
/// Customer segment. Matches Django common.CustomerSegment.
/// </summary>
public class CustomerSegment
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Query { get; set; } = "{}"; // JSON rules
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
}
