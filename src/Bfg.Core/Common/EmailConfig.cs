namespace Bfg.Core.Common;

/// <summary>
/// Per-workspace email config. Matches Django common.EmailConfig.
/// </summary>
public class EmailConfig
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string BackendType { get; set; } = "";
    public string Config { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
}
