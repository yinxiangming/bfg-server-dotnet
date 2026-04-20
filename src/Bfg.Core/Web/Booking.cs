namespace Bfg.Core.Web;

public class BookingTimeSlot
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? SiteId { get; set; }
    public string SlotType { get; set; } = "general";
    public string Name { get; set; } = "";
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int MaxBookings { get; set; } = 1;
    public int CurrentBookings { get; set; }
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class Booking
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? TimeslotId { get; set; }
    public int? CustomerId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Status { get; set; } = "pending";
    public string Notes { get; set; } = "";
    public string AdminNotes { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
