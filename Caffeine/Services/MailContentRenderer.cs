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

            // The general caffeine disclaimer belongs in statistics and announcement emails,
            // not in the email-address confirmation message.
            if (job.Kind != "verify")
                lines.Add(text["MailEstimateNote"]);

            var buttonLabel = job.Kind == "verify" ? (string)text["ConfirmEmailTitle"] : (string)text["OpenKoffi"];
            var plain = subject + "\n\n" + string.Join("\n\n", lines) + "\n\n" + buttonLabel + ": " + cta;
            if (unsubscribe != null)
                plain += "\n\n" + text["Unsubscribe"] + ": " + unsubscribe;

            var html = BuildHtml(MailingPreferenceService.Culture(job.Culture), subject, lines, cta,
                buttonLabel, unsubscribe, text["Unsubscribe"]);
            return new(user.Email, subject, plain, html, unsubscribe, "koffi-job-" + job.Id + "@koffi.hu");
        }
        finally
        {
            CultureInfo.CurrentUICulture = old;
            CultureInfo.CurrentCulture = oldCulture;
        }
    }

    private static string BuildHtml(string culture, string subject, IEnumerable<string> lines,
        string cta, string buttonLabel, string? unsubscribe, string unsubscribeLabel)
    {
        static string Esc(string value) => WebUtility.HtmlEncode(value);
        var paragraphs = string.Join("", lines.Select(line =>
            "<p class=\"koffi-copy\" style=\"line-height:1.6;margin:16px 0;color:#475569\">" +
            Esc(line).Replace("\n", "<br>") + "</p>"));
        var unsubscribeHtml = unsubscribe == null ? "" :
            "<p style=\"font-size:12px;line-height:1.5\"><a class=\"koffi-muted\" style=\"color:#64748b\" href=\"" +
            Esc(unsubscribe) + "\">" + Esc(unsubscribeLabel) + "</a></p>";

        // Light defaults are intentional. Gmail for iOS may recolor HTML messages
        // automatically in dark mode and can disregard media queries. Starting from
        // a dark-only template resulted in Gmail rendering it light in dark mode.
        // Clients honoring prefers-color-scheme get the explicit dark palette.
        return $$"""
            <!doctype html>
            <html lang="{{Esc(culture)}}">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <meta name="color-scheme" content="light dark">
              <meta name="supported-color-schemes" content="light dark">
              <style>
                :root { color-scheme: light dark; supported-color-schemes: light dark; }
                @media (prefers-color-scheme: dark) {
                  .koffi-shell { background-color: #0f172a !important; }
                  .koffi-card { background-color: #1e293b !important; }
                  .koffi-brand { color: #67e8f9 !important; }
                  .koffi-title { color: #f8fafc !important; }
                  .koffi-copy { color: #cbd5e1 !important; }
                  .koffi-button { background-color: #22d3ee !important; color: #083344 !important; }
                  .koffi-muted { color: #94a3b8 !important; }
                }
              </style>
            </head>
            <body class="koffi-shell" style="margin:0;padding:0;background-color:#f1f5f9;font-family:Arial,Helvetica,sans-serif;color:#0f172a">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="#f1f5f9" class="koffi-shell" style="background-color:#f1f5f9">
                <tr><td align="center" style="padding:24px 12px">
                  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" bgcolor="#ffffff" class="koffi-card" style="max-width:560px;background-color:#ffffff;border-radius:20px">
                    <tr><td style="padding:28px">
                      <p class="koffi-brand" style="color:#0e7490;font-size:24px;font-weight:bold;margin:0 0 28px">☕ Koffi</p>
                      <h1 class="koffi-title" style="font-size:24px;line-height:1.3;color:#0f172a;margin:0 0 22px">{{Esc(subject)}}</h1>
                      {{paragraphs}}
                      <p style="margin:28px 0 20px"><a class="koffi-button" style="display:inline-block;background-color:#0891b2;color:#ffffff;padding:14px 22px;border-radius:12px;text-decoration:none;font-weight:bold" href="{{Esc(cta)}}">{{Esc(buttonLabel)}}</a></p>
                      {{unsubscribeHtml}}
                    </td></tr>
                  </table>
                </td></tr>
              </table>
            </body>
            </html>
            """;
    }
}
