namespace Bfg.Core.Common;

/// <summary>
/// User preferences. Matches Django common.UserPreferences.
/// </summary>
public class UserPreferences
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public bool EmailNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; } = true;
    public bool NotifyOrderUpdates { get; set; } = true;
    public bool NotifyPromotions { get; set; } = true;
    public bool NotifyProductUpdates { get; set; }
    public bool NotifySupportReplies { get; set; } = true;
    public string ProfileVisibility { get; set; } = "private";
    public bool ShowEmail { get; set; }
    public bool ShowPhone { get; set; }
    public string Theme { get; set; } = "auto";
    public int ItemsPerPage { get; set; } = 20;
    public string CustomPreferences { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}
