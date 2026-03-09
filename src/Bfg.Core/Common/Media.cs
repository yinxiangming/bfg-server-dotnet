namespace Bfg.Core.Common;

/// <summary>
/// Media file. Matches Django common.Media.
/// </summary>
public class Media
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string? File { get; set; }
    public string ExternalUrl { get; set; } = "";
    public string MediaType { get; set; } = "image";
    public string AltText { get; set; } = "";
    public int? Width { get; set; }
    public int? Height { get; set; }
    public int? UploadedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Workspace Workspace { get; set; } = null!;
    public User? UploadedBy { get; set; }
}
