namespace Caffeine.Services;

// Existing timestamps are local wall times. Keep their meaning; do not reinterpret as UTC.
public sealed class TrackerClock(IConfiguration configuration, TimeProvider timeProvider)
{
    private readonly TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(
        configuration["Tracker:TimeZone"] ?? "Europe/Budapest");

    public DateTime Now =>
        TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), zone).DateTime;
}