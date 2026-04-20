namespace Bfg.Core.Web;

public class Inquiry
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public string Status { get; set; } = "new";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
