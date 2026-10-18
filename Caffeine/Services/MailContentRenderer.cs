using System.Globalization;
using System.Net;
using System.Text.Json;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Caffeine.Services;
public sealed record OutboundMail(string Recipient, string Subject, string PlainText, string Html, string? UnsubscribeUrl = null, string? MessageKey = null);
public sealed record CampaignLanguage(string Title, string Body);
public sealed record CampaignContent(CampaignLanguage Hu, CampaignLanguage En, CampaignLanguage De);
public sealed class MailContentRenderer(AppDbContext db, WeeklySummaryService weeks, DurationDisplay duration, MailingPreferenceService preferences, IStringLocalizer<SharedResource> text, IConfiguration config, IDataProtectionProvider protection, TimeProvider time)
{
    private readonly IDataProtector protector = protection.CreateProtector("Koffi.MailJob.v5");
    public async Task<OutboundMail?> RenderAsync(MailJob job, bool preview = false)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == job.UserId);
        if (user == null || PublicOrigin.Get(config) == null)
            return null;
        var old = CultureInfo.CurrentUICulture;
        var oldCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = new CultureInfo(MailingPreferenceService.Culture(job.Culture));
            string subject;
            var lines = new List<string>();
            string? unsubscribe = null;
            string cta = PublicOrigin.Link(config, "/");
            if (job.Kind == "verify")
            {
                if (user.EmailConfirmed || time.GetUtcNow().UtcDateTime > job.CreatedAtUtc.AddHours(24))
                    return null;
                try
                {
                    cta = protector.Unprotect(job.ProtectedPayload);
                }
                catch
                {
                    return null;
                }

                var token = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(new Uri(cta).Query)["token"].ToString();
                if (!await db.EmailConfirmationTokens.AnyAsync(t => t.UserId == user.Id && t.TokenHash == EmailConfirmationService.Hash(token) && t.UsedAtUtc == null && t.Email == user.Email && t.ExpiresAtUtc > time.GetUtcNow().UtcDateTime))
                    return null;
                subject = text["ConfirmEmailTitle"];
                lines.Add(text["VerifyMailIntro"]);
                lines.Add(text["VerifyMailExpiry"]);
            }
            else
            {
                if (!user.EmailConfirmed)
                    return null;
                var p = await db.MailingPreferences.AsNoTracking().SingleOrDefaultAsync(p => p.UserId == user.Id);
                var weekly = job.Kind == "weekly";
                var test = job.Kind == "campaign-test";
                if (!preview && !test && (p == null || !(weekly ? p.Weekly : p.Announcements)))
                    return null;
                if (p != null)
                    unsubscribe = PublicOrigin.Link(config, "/Account/Unsubscribe?token=" + Uri.EscapeDataString(preferences.Token(p, weekly ? "weekly" : "announcement")) + "&culture=" + MailingPreferenceService.Culture(job.Culture));
                if (weekly)
                {
                    if (!job.WeekStart.HasValue || job.WeekStart.Value.AddDays(7) > SleepStatisticsService.Monday(TimeZoneInfo.ConvertTime(time.GetUtcNow(), TimeZoneInfo.FindSystemTimeZoneById("Europe/Budapest")).DateTime))
                        return null;
                    var s = await weeks.GetAsync(user.Id, job.WeekStart.Value);
                    subject = text["WeeklyMailTitle"] + " · " + s.Start.ToString("d");
                    lines.Add(text["WeeklyMailPeriod", s.Start.ToString("d"), s.Start.AddDays(6).ToString("d")]);
                    if (s.Current.ActiveDays > 0)
                    {
                        lines.Add(text["TotalCaffeine"] + ": " + s.Current.TotalMg.ToString("0.#") + " mg");
                        lines.Add(text["DailyAverage"] + ": " + (s.Current.TotalMg / 7).ToString("0.#") + " mg");
                        lines.Add(text["LoggedDrinks"] + ": " + s.Current.DrinkCount);
                        lines.Add(text["MailLoggedDataNote"]);
                    }
                    else
                        lines.Add(text["MailNoCaffeineData"]);
                    if (s.CanCompare)
                        lines.Add(text["MailPreviousComparison", s.Current.TotalMg.ToString("0.#"), s.Previous.TotalMg.ToString("0.#")]);
                    if (s.Sleep.Count > 0)
                    {
                        lines.Add(text["SleepSample", s.Sleep.Count]);
                        lines.Add(text["SleepAverage"] + ": " + duration.Hours(s.Sleep.AverageHours));
                        lines.Add(text["SleepRating"] + ": " + s.Sleep.AverageRating?.ToString("0.0"));
                        if (s.Sleep.AverageAwakenings.HasValue)
                            lines.Add(text["Awakenings"] + ": " + s.Sleep.AverageAwakenings.Value.ToString("0.0") + " (" + s.Sleep.AwakeningCount + ")");
                    }
                    else
                        lines.Add(text["MailNoSleepData"]);
                    cta = PublicOrigin.Link(config, "/Sleep/Statistics?week=" + s.Start.ToString("yyyy-MM-dd"));
                }
                else
                {
                    var campaign = await db.MailCampaigns.AsNoTracking().SingleOrDefaultAsync(c => c.Id == job.CampaignId);
                    if (campaign == null || (!preview && !test && !campaign.Started))
                        return null;
                    var content = JsonSerializer.Deserialize<CampaignContent>(campaign.ContentJson)!;
                    var localized = job.Culture switch
                    {
                        "hu" => content.Hu,
                        "de" => content.De,
                        _ => content.En
                    };
                    subject = localized.Title;
                    lines.Add(localized.Body);
                }
            }

            lines.Add(text["MailEstimateNote"]);
            var plain = subject + "\n\n" + string.Join("\n\n", lines) + "\n\n" + text["OpenKoffi"] + ": " + cta;
            if (unsubscribe != null)
                plain += "\n\n" + text["Unsubscribe"] + ": " + unsubscribe;
            static string Esc(string s) => WebUtility.HtmlEncode(s);
            var body = string.Join("", lines.Select(l => "<p style=\"line-height:1.6;margin:16px 0;color:#cbd5e1\">" + Esc(l).Replace("\n", "<br>") + "</p>"));
            var html = "<!doctype html><html lang=\"" + MailingPreferenceService.Culture(job.Culture) + "\"><body style=\"margin:0;background:#0f172a;font-family:Arial,sans-serif;color:#f8fafc\"><table role=\"presentation\" width=\"100%\"><tr><td align=\"center\" style=\"padding:24px 12px\"><table role=\"presentation\" width=\"100%\" style=\"max-width:560px;background:#1e293b;border-radius:20px\"><tr><td style=\"padding:28px\"><p style=\"color:#67e8f9;font-size:24px;font-weight:bold\">☕ Koffi</p><h1 style=\"font-size:24px;color:#f8fafc\">" + Esc(subject) + "</h1>" + body + "<p><a style=\"display:inline-block;background:#22d3ee;color:#083344;padding:14px 22px;border-radius:12px;text-decoration:none;font-weight:bold\" href=\"" + Esc(cta) + "\">" + Esc(text["OpenKoffi"]) + "</a></p>" + (unsubscribe == null ? "" : "<p style=\"font-size:12px\"><a style=\"color:#94a3b8\" href=\"" + Esc(unsubscribe) + "\">" + Esc(text["Unsubscribe"]) + "</a></p>") + "</td></tr></table></td></tr></table></body></html>";
            return new(user.Email, subject, plain, html, unsubscribe, "koffi-job-" + job.Id + "@koffi.hu");
        }
        finally
        {
            CultureInfo.CurrentUICulture = old;
            CultureInfo.CurrentCulture = oldCulture;
        }
    }
}
