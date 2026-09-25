namespace Caffeine.Models;

// Calendar day in Tracker:TimeZone, just like existing ConsumedAt timestamps.
// UserId also supports the existing protected guest identity.
public sealed class CaffeineFreeDay
{
    public string UserId { get; set; } = string.Empty;
    public DateTime Day { get; set; }
}