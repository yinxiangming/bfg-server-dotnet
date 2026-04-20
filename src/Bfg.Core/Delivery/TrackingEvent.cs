namespace Bfg.Core.Delivery;

public class TrackingEvent
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? ContentTypeId { get; set; }
    public int? ObjectId { get; set; }
    public int? ConsignmentId { get; set; }
    public int? PackageId { get; set; }
    public string EventType { get; set; } = "";
    public string Description { get; set; } = "";
    public string Location { get; set; } = "";
    public bool IsPublic { get; set; } = true;
    public DateTime EventTime { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? CreatedById { get; set; }
}
