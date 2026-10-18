namespace Caffeine.Services;

// Existing timestamps are local wall times. Keep their meaning; do not reinterpret as UTC.
public sealed class TrackerClock(IConfiguration configuration, TimeProvider timeProvider)
{
    private readonly TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(
        configuration["Tracker:TimeZone"] ?? "Europe/Budapest");

    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;

    public TimeZoneInfo Zone => zone;

    public bool TryUtc(DateTime local, int? offsetMinutes, out DateTime utc)
    {
        local = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        utc = default;

        if (zone.IsInvalidTime(local))
            return false;

        var offsets = zone.IsAmbiguousTime(local)
            ? zone.GetAmbiguousTimeOffsets(local)
            : [zone.GetUtcOffset(local)];

        if (offsets.Length > 1 && !offsetMinutes.HasValue)
            return false;

        var offset = offsetMinutes.HasValue
            ? TimeSpan.FromMinutes(offsetMinutes.Value)
            : offsets[0];

        if (!offsets.Contains(offset))
            return false;

        utc = new DateTimeOffset(local, offset).UtcDateTime;
        return true;
    }

    public DateTime Now =>
        TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), zone).DateTime;
}