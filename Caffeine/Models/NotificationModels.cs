namespace Caffeine.Models;

public sealed class NotificationPreference
{
    public string UserId { get; set; } = "";

    public bool StreakReminder { get; set; }
    public bool SleepCheckIn { get; set; }
    public bool CutoffReminder { get; set; }
    public bool ThresholdReached { get; set; }

    public string StreakTime { get; set; } = "20:00";
    public string MorningTime { get; set; } = "08:00";

    public int CutoffDoseMg { get; set; } = 80;
    public string Culture { get; set; } = "hu";
}

public sealed class BrowserPushSubscription
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";

    public string Endpoint { get; set; } = "";
    public string EndpointHash { get; set; } = "";
    public string P256dh { get; set; } = "";
    public string Auth { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastSeenUtc { get; set; }
}

public sealed class NotificationDelivery
{
    public int Id { get; set; }

    public int SubscriptionId { get; set; }
    public BrowserPushSubscription Subscription { get; set; } = null!;

    public string Kind { get; set; } = "";
    public DateTime Day { get; set; }

    public int Attempts { get; set; }
    public DateTime RetryAfterUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
}