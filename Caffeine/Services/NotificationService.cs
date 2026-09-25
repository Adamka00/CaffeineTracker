using System.Globalization;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Services;

public sealed record DueNotification(
    string Kind,
    string MessageKey,
    string Url);


public sealed class NotificationService(
    AppDbContext db,
    TrackerClock clock,
    TimeProvider time,
    PreferenceService preferences,
    StreakService streaks,
    CaffeinePlanningService planning,
    ICaffeineCalculatorService calculator,
    ICaffeineDecayStrategy decay,
    IPushSender sender,
    IStringLocalizer<SharedResource> text)
{
    public async Task<List<DueNotification>> DueAsync(
        NotificationPreference pref,
        DateTime now)
    {
        var due =
            new List<DueNotification>();


        static bool InWindow(
            DateTime now,
            string hhmm) =>
            PreferenceService.ValidTime(hhmm) &&
            now.TimeOfDay >=
                TimeOnly.ParseExact(
                    hhmm,
                    "HH:mm",
                    CultureInfo.InvariantCulture)
                    .ToTimeSpan() &&
            now.TimeOfDay <
                TimeOnly.ParseExact(
                    hhmm,
                    "HH:mm",
                    CultureInfo.InvariantCulture)
                    .ToTimeSpan()
                    .Add(
                        TimeSpan.FromMinutes(30));


        if (pref.StreakReminder &&
            InWindow(
                now,
                pref.StreakTime) &&
            !(await streaks.GetAsync(
                pref.UserId,
                now))
                .TodayCompleted)
        {
            due.Add(
                new DueNotification(
                    "streak",
                    "PushStreak",
                    "/"));
        }


        if (pref.SleepCheckIn &&
            InWindow(
                now,
                pref.MorningTime) &&
            !await db.SleepLogs.AnyAsync(
                s =>
                    s.UserId ==
                    pref.UserId &&
                    s.SleepDate ==
                    now.Date))
        {
            due.Add(
                new DueNotification(
                    "sleep",
                    "PushSleep",
                    "/Sleep/Edit"));
        }


        if (!pref.CutoffReminder &&
            !pref.ThresholdReached)
        {
            return due;
        }


        var settings =
            await preferences.GetAsync(
                pref.UserId);


        var logs =
            await db.CaffeineLogs
                .AsNoTracking()
                .Where(l =>
                    l.UserId ==
                        pref.UserId &&
                    l.ConsumedAt <=
                        now)
                .ToListAsync();


        var bedtime =
            PreferenceService.NextBedtime(
                now,
                settings.PlannedBedtime);


        var cutoff =
            planning.LatestIntake(
                logs,
                bedtime,
                pref.CutoffDoseMg,
                settings.TargetMg);


        if (pref.CutoffReminder &&
            cutoff.HasValue &&
            cutoff.Value >= now &&
            cutoff.Value <=
                now.AddMinutes(15))
        {
            due.Add(
                new DueNotification(
                    "cutoff",
                    "PushCutoff",
                    "/Planning"));
        }


        var positive =
            logs
                .Where(l =>
                    l.TotalCaffeineMg > 0)
                .ToArray();


        if (pref.ThresholdReached &&
            positive.Length > 0)
        {
            var peak =
                positive.Max(
                    l => l.ConsumedAt)
                +
                decay.TimeToPeak;


            if (calculator
                    .GetCurrentTotalActiveCaffeine(
                        positive,
                        peak)
                >
                settings.TargetMg)
            {
                var crossing =
                    planning.UnderTarget(
                        positive,
                        peak,
                        settings.TargetMg);


                if (crossing.HasValue &&
                    crossing.Value <= now &&
                    crossing.Value >=
                        now.AddMinutes(-10))
                {
                    due.Add(
                        new DueNotification(
                            "threshold",
                            "PushThreshold",
                            "/"));
                }
            }
        }


        return due;
    }


    public async Task RunOnceAsync(
        CancellationToken token)
    {
        var now =
            clock.Now;

        var utc =
            time
                .GetUtcNow()
                .UtcDateTime;


        var preferenceRows =
            await db.NotificationPreferences
                .AsNoTracking()
                .Where(p =>
                    (
                        p.StreakReminder ||
                        p.SleepCheckIn ||
                        p.CutoffReminder ||
                        p.ThresholdReached
                    )
                    &&
                    db.BrowserPushSubscriptions
                        .Any(s =>
                            s.UserId ==
                            p.UserId))
                .ToListAsync(token);


        foreach (var pref in preferenceRows)
        {
            var due =
                await DueAsync(
                    pref,
                    now);


            var subscriptions =
                await db.BrowserPushSubscriptions
                    .Where(s =>
                        s.UserId ==
                        pref.UserId)
                    .ToListAsync(token);


            foreach (var subscription
                     in subscriptions)
            {
                foreach (var item in due)
                {
                    await db.Database
                        .ExecuteSqlInterpolatedAsync(
                            $"""
                            INSERT OR IGNORE INTO NotificationDeliveries
                                (SubscriptionId, Kind, Day, Attempts, RetryAfterUtc)
                            VALUES
                                ({subscription.Id}, {item.Kind}, {now.Date}, 0, {utc});
                            """,
                            token);


                    // Persist a short claim before sending.
                    // One container, and restart-safe bounded retries.
                    var claimed =
                        await db.NotificationDeliveries
                            .Where(d =>
                                d.SubscriptionId ==
                                    subscription.Id &&
                                d.Kind ==
                                    item.Kind &&
                                d.Day ==
                                    now.Date &&
                                d.SentAtUtc ==
                                    null &&
                                d.Attempts <
                                    3 &&
                                d.RetryAfterUtc <=
                                    utc)
                            .ExecuteUpdateAsync(
                                s =>
                                    s.SetProperty(
                                            d =>
                                                d.Attempts,
                                            d =>
                                                d.Attempts +
                                                1)
                                        .SetProperty(
                                            d =>
                                                d.RetryAfterUtc,
                                            utc.AddMinutes(
                                                2)),
                                token);


                    if (claimed == 0)
                        continue;


                    var oldCulture =
                        CultureInfo.CurrentUICulture;

                    string body;


                    try
                    {
                        CultureInfo.CurrentUICulture =
                            CultureInfo.GetCultureInfo(
                                pref.Culture ==
                                "en"
                                    ? "en"
                                    : "hu");

                        body =
                            text[item.MessageKey];
                    }
                    finally
                    {
                        CultureInfo.CurrentUICulture =
                            oldCulture;
                    }


                    var result =
                        await sender.SendAsync(
                            subscription,
                            body,
                            item.Url,
                            $"koffi-{item.Kind}-{now:yyyyMMdd}",
                            token);


                    if (result ==
                        PushResult.Gone)
                    {
                        await db
                            .BrowserPushSubscriptions
                            .Where(s =>
                                s.Id ==
                                subscription.Id)
                            .ExecuteDeleteAsync(
                                token);

                        break;
                    }


                    if (result ==
                        PushResult.Sent)
                    {
                        await db
                            .NotificationDeliveries
                            .Where(d =>
                                d.SubscriptionId ==
                                    subscription.Id &&
                                d.Kind ==
                                    item.Kind &&
                                d.Day ==
                                    now.Date)
                            .ExecuteUpdateAsync(
                                s =>
                                    s.SetProperty(
                                        d =>
                                            d.SentAtUtc,
                                        utc),
                                token);
                    }
                }
            }
        }


        await db.NotificationDeliveries
            .Where(d =>
                d.Day <
                now.Date.AddDays(-30))
            .ExecuteDeleteAsync(token);


        await db.PasswordResetTokens
            .Where(t =>
                t.ExpiresAt <
                utc.AddDays(-1))
            .ExecuteDeleteAsync(token);
    }
}


public sealed class NotificationWorker(
    IServiceScopeFactory scopes,
    PushConfiguration config,
    TimeProvider time,
    ILogger<NotificationWorker> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        if (!config.IsConfigured)
            return;


        using var timer =
            new PeriodicTimer(
                TimeSpan.FromMinutes(1),
                time);


        while (!stoppingToken
                   .IsCancellationRequested)
        {
            try
            {
                using var scope =
                    scopes.CreateScope();


                await scope.ServiceProvider
                    .GetRequiredService<
                        NotificationService>()
                    .RunOnceAsync(
                        stoppingToken);
            }
            catch (OperationCanceledException)
                when (
                    stoppingToken
                        .IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                logger.LogWarning(
                    "Notification cycle failed; " +
                    "next cycle will retry. " +
                    "No user data was logged.");
            }


            if (!await timer
                    .WaitForNextTickAsync(
                        stoppingToken))
            {
                break;
            }
        }
    }
}