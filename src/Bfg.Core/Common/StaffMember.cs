namespace Bfg.Core.Common;

/// <summary>
/// Staff member (User + Workspace + Role). Matches Django common.StaffMember.
/// </summary>
public class StaffMember
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int UserId { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User User { get; set; } = null!;
    public StaffRole Role { get; set; } = null!;
}
