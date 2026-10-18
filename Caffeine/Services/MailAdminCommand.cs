using System.Text.Json;
using System.Text.RegularExpressions;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
// Local operator CLI only. No admin HTTP endpoint, credentials, or public recipient selector.
public static class MailAdminCommand
{
    public static async Task<int> RunAsync(string[] args, IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var time = scope.ServiceProvider.GetRequiredService<TimeProvider>();
        string? Value(string flag)
        {
            var i = Array.IndexOf(args, flag);
            return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
        }

        try
        {
            var op = args[0];
            var key = Value("--key");
            if (op == "--mail-status")
            {
                foreach (var g in await db.MailJobs.GroupBy(j => j.State).Select(g => new { State = g.Key, Count = g.Count() }).ToListAsync())
                    Console.WriteLine(g.State + ": " + g.Count);
                return 0;
            }

            if (op == "--mail-retry" || op == "--mail-resolve")
            {
                if (!int.TryParse(Value("--job"), out var id) || Value("--confirm") != id.ToString())
                    throw new ArgumentException();
                var job = await db.MailJobs.SingleOrDefaultAsync(j => j.Id == id);
                if (job == null)
                    throw new ArgumentException();
                if (op == "--mail-retry")
                {
                    if (job.State != "Failed" || job.Kind == "verify")
                        throw new ArgumentException();
                    job.State = "Pending";
                    job.Attempts = 0;
                    job.NextAttemptUtc = time.GetUtcNow().UtcDateTime;
                }
                else
                {
                    if (job.State != "Uncertain" || Value("--state")is not ("sent" or "cancelled"))
                        throw new ArgumentException();
                    job.State = Value("--state") == "sent" ? "Sent" : "Cancelled";
                    if (job.State == "Sent")
                        job.SentAtUtc = time.GetUtcNow().UtcDateTime;
                }

                job.LeaseId = null;
                await db.SaveChangesAsync();
                Console.WriteLine("Job state updated; no email was sent by this command.");
                return 0;
            }

            if (op == "--mail-campaign-create")
            {
                if (key == null || !Regex.IsMatch(key, "^[a-zA-Z0-9_-]{1,60}$") || Value("--content") == null)
                    throw new ArgumentException();
                var raw = await File.ReadAllTextAsync(Value("--content")!);
                if (raw.Length > 100000)
                    throw new ArgumentException();
                var content = JsonSerializer.Deserialize<CampaignContent>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? throw new ArgumentException();
                foreach (var c in new[]
                {
                    content.Hu,
                    content.En,
                    content.De
                }

                )
                    if (c == null || string.IsNullOrWhiteSpace(c.Title) || c.Title.Length > 200 || c.Title.Any(char.IsControl) || string.IsNullOrWhiteSpace(c.Body) || c.Body.Length > 20000)
                        throw new ArgumentException();
                db.MailCampaigns.Add(new() { Key = key, ContentJson = JsonSerializer.Serialize(content), CreatedAtUtc = time.GetUtcNow().UtcDateTime });
                await db.SaveChangesAsync();
                Console.WriteLine("Draft created.");
                return 0;
            }

            if (op == "--mail-campaign-send")
            {
                var c = await db.MailCampaigns.SingleOrDefaultAsync(c => c.Key == key);
                if (c == null || Value("--confirm") != key || c.TestSentAtUtc == null)
                    throw new ArgumentException();
                c.Started = true;
                await db.SaveChangesAsync();
                var outbox = scope.ServiceProvider.GetRequiredService<MailOutboxService>();
                var preferences = await db.MailingPreferences.AsNoTracking().Where(p => p.Announcements && p.User.EmailConfirmed).ToListAsync();
                foreach (var p in preferences)
                    await outbox.EnqueueAsync(p.UserId, "announcement", "campaign:" + c.Id, p.Culture, null, c.Id);
                Console.WriteLine("Eligible recipients queued: " + preferences.Count);
                return 0;
            }

            if (op == "--mail-preview" || op == "--mail-test")
            {
                if (!int.TryParse(Value("--user"), out var userId))
                    throw new ArgumentException();
                var campaign = key == null ? null : await db.MailCampaigns.AsNoTracking().SingleOrDefaultAsync(c => c.Key == key);
                var isWeekly = Value("--week") != null;
                DateTime? week = null;
                if (isWeekly)
                {
                    if (!DateTime.TryParseExact(Value("--week"), "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var day) || day.Year < 2000)
                        throw new ArgumentException();
                    week = SleepStatisticsService.Monday(day);
                }

                if (!isWeekly && campaign == null)
                    throw new ArgumentException();
                var job = new MailJob
                {
                    UserId = userId,
                    Kind = isWeekly ? "weekly" : "campaign-test",
                    WeekStart = week,
                    CampaignId = campaign?.Id,
                    Culture = MailingPreferenceService.Culture(Value("--culture"))
                };
                var message = await scope.ServiceProvider.GetRequiredService<MailContentRenderer>().RenderAsync(job, true);
                if (message == null)
                    throw new ArgumentException();
                if (op == "--mail-preview")
                {
                    var output = Value("--output") ?? throw new ArgumentException();
                    await File.WriteAllTextAsync(output, message.Html);
                    await File.WriteAllTextAsync(output + ".txt", message.PlainText);
                    Console.WriteLine("Preview written. Treat as private user data.");
                }
                else
                {
                    if (isWeekly)
                        throw new ArgumentException();
                    await scope.ServiceProvider.GetRequiredService<MailOutboxService>().EnqueueAsync(userId, "campaign-test", "test:" + Guid.NewGuid().ToString("N"), job.Culture, null, campaign!.Id);
                    Console.WriteLine("Test queued for the selected verified account.");
                }

                return 0;
            }

            return 2;
        }
        catch (Exception)
        {
            Console.Error.WriteLine("Mail command failed. Check arguments, database, verified account and draft/test state. No private values logged.");
            return 2;
        }
    }
}
