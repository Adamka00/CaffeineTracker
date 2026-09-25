using System.Globalization;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed class PreferenceService(AppDbContext db)
{
    public async Task<TrackerPreference> GetAsync(
        string userId,
        string? legacyBedtime = null) =>
        await db.TrackerPreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(p => p.UserId == userId)
        ?? new()
        {
            UserId = userId,
            PlannedBedtime =
                ValidTime(legacyBedtime)
                    ? legacyBedtime!
                    : "23:00"
        };

    public static bool ValidTime(string? value) =>
        TimeOnly.TryParseExact(
            value,
            "HH:mm",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);

    public static DateTime NextBedtime(
        DateTime now,
        string time)
    {
        var at = now.Date.Add(
            TimeOnly.ParseExact(
                    time,
                    "HH:mm",
                    CultureInfo.InvariantCulture)
                .ToTimeSpan());

        return at <= now
            ? at.AddDays(1)
            : at;
    }

    public async Task SaveAsync(
        string userId,
        string bedtime,
        double target)
    {
        if (!ValidTime(bedtime) ||
            !double.IsFinite(target) ||
            target < 1 ||
            target > 200)
        {
            throw new ArgumentException(
                "Invalid preference.");
        }

        await db.Database.ExecuteSqlInterpolatedAsync($"""
                                                       INSERT INTO TrackerPreferences
                                                           (UserId, PlannedBedtime, TargetMg)
                                                       VALUES
                                                           ({userId}, {bedtime}, {target})
                                                       ON CONFLICT(UserId) DO UPDATE SET
                                                           PlannedBedtime = excluded.PlannedBedtime,
                                                           TargetMg = excluded.TargetMg;
                                                       """);
    }
}