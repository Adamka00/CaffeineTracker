using Caffeine.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Caffeine.Tests;

public class StreakTests
{
    [Theory]
    [InlineData("2026-09-17", "", 0, 0, false)]
    [InlineData("2026-09-17", "2026-09-17,2026-09-17", 1, 1, true)]
    [InlineData("2026-09-17", "2026-09-15,2026-09-16", 2, 2, false)]
    [InlineData("2026-09-18", "2026-09-15,2026-09-16", 0, 2, false)]
    [InlineData("2026-09-17", "2026-09-10,2026-09-11,2026-09-12,2026-09-17", 1, 3, true)]
    [InlineData("2026-09-17", "2026-09-18", 0, 0, false)]
    [InlineData("2026-01-01", "2025-12-30,2025-12-31,2026-01-01", 3, 3, true)]
    [InlineData("2024-03-01", "2024-02-28,2024-02-29,2024-03-01", 3, 3, true)]
    [InlineData("2026-03-30", "2026-03-28,2026-03-29,2026-03-30", 3, 3, true)]
    [InlineData("2026-10-26", "2026-10-24,2026-10-25,2026-10-26", 3, 3, true)]
    public void CalendarDaysAreDistinctAndRespectGaps(
        string today,
        string dates,
        int current,
        int best,
        bool done)
    {
        var actual =
            StreakService.Calculate(
                dates
                    .Split(
                        ',',
                        StringSplitOptions.RemoveEmptyEntries)
                    .Select(
                        d => DateTime.Parse(
                            d,
                            System.Globalization.CultureInfo
                                .InvariantCulture)),
                DateTime.Parse(
                    today,
                    System.Globalization.CultureInfo
                        .InvariantCulture));

        Assert.Equal(
            new StreakSummary(
                current,
                best,
                done),
            actual);
    }


    [Theory]
    [InlineData("2026-09-17T21:59:59Z", "2026-09-17")]
    [InlineData("2026-09-17T22:00:00Z", "2026-09-18")]
    [InlineData("2026-03-29T01:00:00Z", "2026-03-29")]
    [InlineData("2026-10-25T01:00:00Z", "2026-10-25")]
    public void DayBoundaryUsesConfiguredBudapestTimezone(
        string utc,
        string expectedDay)
    {
        var clock =
            new TrackerClock(
                new ConfigurationBuilder()
                    .Build(),
                new AtTime(
                    DateTimeOffset.Parse(
                        utc)));

        Assert.Equal(
            expectedDay,
            clock.Now.ToString(
                "yyyy-MM-dd"));
    }


    private sealed class AtTime(
        DateTimeOffset at)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            at;
    }
}