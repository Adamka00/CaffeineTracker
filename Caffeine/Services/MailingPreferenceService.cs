using System.Text.Json;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
public sealed record UnsubscribeLink(int UserId, string Kind, string Version, DateTime ExpiresUtc);
public sealed class MailingPreferenceService(AppDbContext db, TimeProvider time, IDataProtectionProvider protection)
{
    private readonly IDataProtector protector = protection.CreateProtector("Koffi.Unsubscribe.v5");
    public async Task<bool> SaveAsync(int userId, bool announcements, bool weekly, string culture)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == userId);
        if (user == null || ((announcements || weekly) && !user.EmailConfirmed))
            return false;
        var p = await db.MailingPreferences.SingleOrDefaultAsync(p => p.UserId == userId);
        if (p == null)
        {
            p = new()
            {
                UserId = userId
            };
            db.MailingPreferences.Add(p);
        }

        var now = time.GetUtcNow().UtcDateTime;
        void Audit(string kind, bool enabled) => db.MailingConsents.Add(new() { UserId = userId, Kind = kind, Enabled = enabled, AtUtc = now });
        if (p.Announcements != announcements)
        {
            Audit("announcement", announcements);
            p.Announcements = announcements;
            p.AnnouncementVersion = Guid.NewGuid().ToString("N");
            p.AnnouncementConsentUtc = announcements ? now : null;
        }

        if (p.Weekly != weekly)
        {
            Audit("weekly", weekly);
            p.Weekly = weekly;
            p.WeeklyVersion = Guid.NewGuid().ToString("N");
            p.WeeklyConsentUtc = weekly ? now : null;
        }

        p.Culture = Culture(culture);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return true;
    }

    public static string Culture(string? c) => c is "hu" or "de" ? c : "en";
    public string Token(MailingPreference p, string kind) => protector.Protect(JsonSerializer.Serialize(new UnsubscribeLink(p.UserId, kind, kind == "weekly" ? p.WeeklyVersion : p.AnnouncementVersion, time.GetUtcNow().UtcDateTime.AddYears(1))));
    public UnsubscribeLink? Read(string? token)
    {
        if (token == null || token.Length > 4000)
            return null;
        try
        {
            var p = JsonSerializer.Deserialize<UnsubscribeLink>(protector.Unprotect(token));
            return p != null && (p.Kind is "weekly" or "announcement") && p.ExpiresUtc > time.GetUtcNow().UtcDateTime ? p : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> UnsubscribeAsync(string token)
    {
        var link = Read(token);
        if (link == null)
            return false;
        await using var tx = await db.Database.BeginTransactionAsync();
        var p = await db.MailingPreferences.SingleOrDefaultAsync(p => p.UserId == link.UserId);
        if (p == null)
            return false;
        if ((link.Kind == "weekly" ? p.WeeklyVersion : p.AnnouncementVersion) != link.Version)
            return false;
        var enabled = link.Kind == "weekly" ? p.Weekly : p.Announcements;
        if (enabled)
        {
            if (link.Kind == "weekly")
            {
                p.Weekly = false;
            }
            else
            {
                p.Announcements = false;
            }

            db.MailingConsents.Add(new() { UserId = p.UserId, Kind = link.Kind, Enabled = false, AtUtc = time.GetUtcNow().UtcDateTime });
            await db.SaveChangesAsync();
        }

        await tx.CommitAsync();
        return true;
    }
}

public static class PublicOrigin
{
    public static Uri? Get(IConfiguration c) => Uri.TryCreate(c["Account:PublicOrigin"], UriKind.Absolute, out var uri) && uri.Scheme == "https" && uri.AbsolutePath == "/" && string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment) ? uri : null;
    public static string Link(IConfiguration c, string path) => Get(c)?.GetLeftPart(UriPartial.Authority) + path;
}
