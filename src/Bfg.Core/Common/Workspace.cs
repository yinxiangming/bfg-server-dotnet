namespace Bfg.Core.Common;

/// <summary>
/// Multi-tenancy workspace. Matches Django common.Workspace.
/// </summary>
public class Workspace
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Uuid { get; set; } = "";
    public string Slug { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public string Settings { get; set; } = "{}"; // JSON
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
