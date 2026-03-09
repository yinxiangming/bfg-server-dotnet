namespace Bfg.Core.Common;

/// <summary>
/// User entity. Matches Django common.User (extends AbstractUser).
/// </summary>
public class User
{
    public int Id { get; set; }
    public string Password { get; set; } = "";
    public DateTime? LastLogin { get; set; }
    public bool IsSuperuser { get; set; }
    public string Username { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsStaff { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime DateJoined { get; set; }
    public string Phone { get; set; } = "";
    public string Avatar { get; set; } = ""; // upload path
    public int? DefaultWorkspaceId { get; set; }
    public string Language { get; set; } = "en";
    public string TimezoneName { get; set; } = "UTC";
    public DateTime UpdatedAt { get; set; }

    public Workspace? DefaultWorkspace { get; set; }
}
