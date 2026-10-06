using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed record KoffiRelease(string Version, string[] FeatureKeys);

public static class ReleaseCatalog
{
    public static readonly KoffiRelease[] All = [
        new("4.0", ["Release4Ux", "Release4Insights", "Release4Languages", "Release4Barcode"]),
        new("3.0", ["Release3Sleep", "Release3Planner", "Release3Stats"]),
        new("2.0", ["Release2History", "Release2Guest"])
    ];

    public static KoffiRelease Current => All[0];

    public static bool IsKnown(string version) =>
        All.Any(r => r.Version == version);

    public static string Newer(string a, string b) =>
        Version.TryParse(a, out var av) && Version.TryParse(b, out var bv)
            ? (av >= bv ? a : b)
            : Version.TryParse(a, out _) ? a : b;
}

public sealed record ExperienceState(
    bool ShowTutorial,
    bool ShowRelease,
    bool ShowMorning,
    string ClientKey);

public sealed class ExperienceService(AppDbContext db, TrackerClock clock)
{
    public static bool MorningDue(DateTime now, bool logged, DateTime? dismissed) =>
        now.Hour is >= 6 and < 12 &&
        !logged &&
        dismissed?.Date != now.Date;

    public static bool ReleaseDue(string seen) =>
        seen != ReleaseCatalog.Current.Version;

    public async Task<ExperienceState> GetAsync(string user)
    {
        var p = await db.ExperiencePreferences
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == user);

        var now = clock.Now;

        var logged = await db.SleepLogs.AnyAsync(s =>
            s.UserId == user &&
            s.SleepDate == now.Date &&
            !s.IsImportedDuplicate);

        var key = Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(user)))[..24];

        return new(
            p?.TutorialCompleted != true,
            ReleaseDue(p?.LastSeenRelease ?? ""),
            MorningDue(now, logged, p?.MorningDismissedDate),
            key);
    }

    public async Task SaveAsync(
        string user,
        string action,
        string? version = null)
    {
        if (action is not ("tutorial" or "release" or "morning") ||
            (action == "release" &&
             !ReleaseCatalog.IsKnown(version ?? "")))
        {
            throw new ArgumentException("Invalid experience action");
        }

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO ExperiencePreferences (UserId, TutorialCompleted, LastSeenRelease, MorningDismissedDate) VALUES ({user}, {false}, {""}, NULL) ON CONFLICT(UserId) DO NOTHING");

        var query = db.ExperiencePreferences
            .Where(p => p.UserId == user);

        if (action == "tutorial")
        {
            await query.ExecuteUpdateAsync(s =>
                s.SetProperty(p => p.TutorialCompleted, true));
        }

        if (action == "release")
        {
            var old = await query
                .Select(p => p.LastSeenRelease)
                .SingleAsync();

            var latest = ReleaseCatalog.Newer(old, version!);

            await query.ExecuteUpdateAsync(s =>
                s.SetProperty(p => p.LastSeenRelease, latest));
        }

        if (action == "morning")
        {
            await query.ExecuteUpdateAsync(s =>
                s.SetProperty(
                    p => p.MorningDismissedDate,
                    clock.Now.Date));
        }
    }
}