namespace Bfg.Core.Support;

public class TicketCategory
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TicketPriority
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; } = 1;
    public string Color { get; set; } = "#000000";
    public int ResponseTimeHours { get; set; } = 24;
    public int ResolutionTimeHours { get; set; } = 72;
    public bool IsActive { get; set; } = true;
}
