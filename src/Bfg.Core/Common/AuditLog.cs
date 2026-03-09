namespace Bfg.Core.Common;

/// <summary>
/// Audit log. Matches Django common.AuditLog.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }
    public int? WorkspaceId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = "";
    public string Description { get; set; } = "";
    public int? ContentTypeId { get; set; }
    public int? ObjectId { get; set; }
    public string ObjectRepr { get; set; } = "";
    public string Changes { get; set; } = "{}";
    public string? IpAddress { get; set; }
    public string UserAgent { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
