namespace Bfg.Core.Common;

/// <summary>
/// Generic link between Media and any model. Matches Django common.MediaLink.
/// </summary>
public class MediaLink
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public int ContentTypeId { get; set; }
    public int ObjectId { get; set; }
    public int Position { get; set; } = 100;
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Media Media { get; set; } = null!;
}
