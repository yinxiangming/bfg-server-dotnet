namespace Bfg.Core.Common;

public class ApiKey
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? UserId { get; set; }
    public string Name { get; set; } = "";
    public string KeyPrefix { get; set; } = "";
    public string Prefix { get; set; } = "";
    public string KeyHash { get; set; } = "";
    public string Permissions { get; set; } = "[]";
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
