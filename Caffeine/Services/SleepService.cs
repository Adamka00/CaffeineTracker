using Caffeine.Data;
using Caffeine.Models;
using Caffeine.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed class SleepService(
    AppDbContext db,
    ICaffeineCalculatorService calculator,
    TrackerClock clock)
{
    public async Task<string?> SaveAsync(
        string userId,
        SleepForm form)
    {
        var now = clock.Now;

        if (form.ActualBedtime is not { } bed ||
            form.WakeTime is not { } wake ||
            bed < new DateTime(2000, 1, 1) ||
            wake > now ||
            wake <= bed ||
            (wake - bed).TotalHours > 24 ||
            form.SleepRating is < 1 or > 5 ||
            form.FallingAsleep is < 1 or > 5 ||
            form.Awakenings is < 0 or > 50 ||
            form.Notes?.Length > 2000 ||
            (form.PlannedBedtime is { } planned &&
             (planned < wake.AddHours(-36) ||
              planned > wake)))
        {
            return "InvalidSleep";
        }

        var entry =
            form.Id == 0
                ? new SleepLog { UserId = userId }
                : await db.SleepLogs
                    .SingleOrDefaultAsync(s =>
                        s.Id == form.Id &&
                        s.UserId == userId);

        if (entry == null)
            return "NotFound";

        if (!entry.IsImportedDuplicate &&
            await db.SleepLogs.AnyAsync(s =>
                s.UserId == userId &&
                s.SleepDate == wake.Date &&
                s.Id != form.Id &&
                !s.IsImportedDuplicate))
        {
            return "SleepDuplicate";
        }

        var previousDay = wake.Date.AddDays(-1);

        var logs = await db.CaffeineLogs
            .AsNoTracking()
            .Where(l =>
                l.UserId == userId &&
                l.ConsumedAt <= now)
            .ToListAsync();

        entry.SleepDate = wake.Date;
        entry.ActualBedtime = bed;
        entry.WakeTime = wake;
        entry.PlannedBedtime = form.PlannedBedtime;
        entry.SleepRating = form.SleepRating;
        entry.FallingAsleep = form.FallingAsleep;
        entry.Awakenings = form.Awakenings;
        entry.Notes =
            string.IsNullOrWhiteSpace(form.Notes)
                ? null
                : form.Notes.Trim();

        entry.EstimatedCaffeineAtBedtime =
            Math.Round(
                calculator.GetCurrentTotalActiveCaffeine(
                    logs,
                    bed),
                2);

        entry.PreviousDayCaffeineMg =
            logs.Where(l =>
                    l.ConsumedAt >= previousDay &&
                    l.ConsumedAt < wake.Date)
                .Sum(l => l.TotalCaffeineMg);

        entry.LastCaffeineAt =
            logs.Where(l =>
                    l.TotalCaffeineMg > 0 &&
                    l.ConsumedAt <= bed)
                .Select(l => (DateTime?)l.ConsumedAt)
                .Max();

        if (form.Id == 0)
            db.SleepLogs.Add(entry);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is
                Microsoft.Data.Sqlite.SqliteException
                { SqliteErrorCode: 19 })
        {
            db.ChangeTracker.Clear();
            return "SleepDuplicate";
        }

        return null;
    }
}