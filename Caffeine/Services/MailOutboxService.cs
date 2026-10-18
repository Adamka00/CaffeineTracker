using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
// Transport explicitly distinguishes known rejection from an ambiguous SMTP outcome.
public sealed class MailDeliveryException(string outcome) : Exception("Mail delivery " + outcome)
{
    public string Outcome { get; } = outcome;
}

public sealed class MailOutboxService(AppDbContext db, TimeProvider time, MailContentRenderer renderer, IEmailSender sender, IConfiguration config)
{
    public async Task CleanupAsync()
    {
        var now = time.GetUtcNow().UtcDateTime;
        await db.EmailConfirmationTokens.Where(t => t.ExpiresAtUtc < now.AddDays(-7)).ExecuteDeleteAsync();
        await db.MailJobs.Where(j => j.Kind == "verify" && j.CreatedAtUtc < now.AddDays(-30) &&
            (j.State == "Sent" || j.State == "Cancelled" || j.State == "Failed" || j.State == "Uncertain")).ExecuteDeleteAsync();
        // Weekly/campaign tombstones remain, so re-running a campaign cannot re-send old recipients.
    }

    public async Task QueueWeeklyAsync()
    {
        var local = TimeZoneInfo.ConvertTime(time.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest")).DateTime;
        var day = Math.Clamp(config.GetValue("Mailing:WeeklyDay", 1), 0, 6);
        var send = TimeOnly.TryParseExact(config["Mailing:WeeklyTime"] ?? "08:00", "HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var at) ? at.ToTimeSpan() : TimeSpan.FromHours(8);
        var week = WeeklySummaryService.LatestDueWeek(local, day, send);
        var key = "weekly:" + week.ToString("yyyy-MM-dd");
        var last = 0;
        while (true)
        {
            var batch = await db.MailingPreferences.AsNoTracking().Where(p => p.UserId > last && p.Weekly && p.User.EmailConfirmed).OrderBy(p => p.UserId).Take(200).ToListAsync();
            if (batch.Count == 0)
                break;
            foreach (var p in batch)
            {
                // Opt-in after the scheduled week closed starts with the next completed week, not old history.
                if (p.WeeklyConsentUtc.HasValue && p.WeeklyConsentUtc.Value > ToUtc(week.AddDays(7)))
                    continue;
                await EnqueueAsync(p.UserId, "weekly", key, p.Culture, week, null);
            }

            last = batch[^1].UserId;
        }
    }

    private static DateTime ToUtc(DateTime local) => TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest"));
    public async Task EnqueueAsync(int user, string kind, string key, string culture, DateTime? week, int? campaign)
    {
        var now = time.GetUtcNow().UtcDateTime;
        await db.Database.ExecuteSqlInterpolatedAsync($"""
    INSERT INTO MailJobs (UserId,Kind,DeduplicationKey,ProtectedPayload,Culture,CampaignId,WeekStart,State,Attempts,CreatedAtUtc,NextAttemptUtc)
    VALUES ({user},{kind},{key},{""},{culture},{campaign},{week},{"Pending"},{0},{now},{now})
    ON CONFLICT(UserId,DeduplicationKey) DO NOTHING
    """);
    }

    public async Task<int> RecoverLeasesAsync()
    {
        var now = time.GetUtcNow().UtcDateTime;
        var uncertain = await db.MailJobs.Where(j => j.State == "Sending" && j.LeaseUntilUtc < now).ExecuteUpdateAsync(s => s.SetProperty(j => j.State, "Uncertain").SetProperty(j => j.ProtectedPayload, ""));
        await db.MailJobs.Where(j => j.State == "Claimed" && j.LeaseUntilUtc < now).ExecuteUpdateAsync(s => s.SetProperty(j => j.State, "Pending").SetProperty(j => j.LeaseId, (string? )null));
        return uncertain;
    }

    public async Task<bool> RunOneAsync(CancellationToken token = default)
    {
        if (!sender.IsConfigured || PublicOrigin.Get(config) == null)
            return false;
        await RecoverLeasesAsync();
        var now = time.GetUtcNow().UtcDateTime;
        var id = await db.MailJobs.AsNoTracking().Where(j => j.State == "Pending" && j.NextAttemptUtc <= now).OrderBy(j => j.Kind == "verify" ? 0 : j.Kind == "weekly" ? 1 : 2).ThenBy(j => j.Id).Select(j => (int? )j.Id).FirstOrDefaultAsync(token);
        if (!id.HasValue)
            return false;
        var lease = Guid.NewGuid().ToString("N");
        var until = now.AddMinutes(2);
        if (await db.MailJobs.Where(j => j.Id == id && j.State == "Pending" && j.NextAttemptUtc <= now).ExecuteUpdateAsync(s => s.SetProperty(j => j.State, "Claimed").SetProperty(j => j.LeaseId, lease).SetProperty(j => j.LeaseUntilUtc, until), token) == 0)
            return true;
        var job = await db.MailJobs.AsNoTracking().SingleAsync(j => j.Id == id, token);
        var message = await renderer.RenderAsync(job);
        if (message == null)
        {
            await Complete("Cancelled", false);
            return true;
        }

        if (await db.MailJobs.Where(j => j.Id == id && j.State == "Claimed" && j.LeaseId == lease).ExecuteUpdateAsync(s => s.SetProperty(j => j.State, "Sending").SetProperty(j => j.Attempts, j => j.Attempts + 1), token) == 0)
            return true;
        try
        {
            await sender.SendMessageAsync(message, token);
            await Complete("Sent", true);
            if (job.Kind == "campaign-test" && job.CampaignId.HasValue)
                await db.MailCampaigns.Where(c => c.Id == job.CampaignId).ExecuteUpdateAsync(s => s.SetProperty(c => c.TestSentAtUtc, time.GetUtcNow().UtcDateTime));
        }
        catch (MailDeliveryException ex)
        {
            if (ex.Outcome == "retry" && job.Attempts < 4)
            {
                var next = time.GetUtcNow().UtcDateTime.AddMinutes(Math.Pow(2, job.Attempts));
                await db.MailJobs.Where(j => j.Id == id && j.LeaseId == lease).ExecuteUpdateAsync(s => s.SetProperty(j => j.State, "Pending").SetProperty(j => j.NextAttemptUtc, next).SetProperty(j => j.LeaseId, (string? )null));
            }
            else
                await Complete(ex.Outcome == "unknown" ? "Uncertain" : "Failed", false);
        }
        catch (OperationCanceledException)
        {
            await Complete("Uncertain", false);
            if (token.IsCancellationRequested)
                throw;
        }
        catch (Exception)
        {
            await Complete("Uncertain", false);
        }

        return true;
        async Task Complete(string state, bool sent)
        {
            var at = time.GetUtcNow().UtcDateTime;
            await db.MailJobs.Where(j => j.Id == id && j.LeaseId == lease).ExecuteUpdateAsync(s => s.SetProperty(j => j.State, state).SetProperty(j => j.SentAtUtc, sent ? (DateTime? )at : null).SetProperty(j => j.ProtectedPayload, "").SetProperty(j => j.LeaseId, (string? )null));
        }
    }
}

public sealed class MailingWorker(IServiceScopeFactory scopes, IConfiguration config, TimeProvider time, ILogger<MailingWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Opt-in operational switch prevents unexpected mass delivery during a manual deploy.
        if (!config.GetValue<bool>("Mailing:Enabled"))
            return;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopes.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<MailOutboxService>();
                await service.CleanupAsync();
                await service.QueueWeeklyAsync();
                for (var i = 0; i < 20 && !stoppingToken.IsCancellationRequested; i++)
                {
                    if (!await service.RunOneAsync(stoppingToken))
                        break;
                    await Task.Delay(TimeSpan.FromMilliseconds(Math.Clamp(config.GetValue("Mailing:IntervalMs", 1000), 250, 60000)), time, stoppingToken);
                }
            }
            catch (OperationCanceledException)when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                log.LogWarning("Mail worker cycle failed; pending jobs remain stored. No payload or address logged.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), time, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
