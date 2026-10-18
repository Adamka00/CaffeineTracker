using System.Security.Cryptography;
using System.Text;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;
public sealed class EmailConfirmationService(AppDbContext db, TimeProvider time, IConfiguration config, IEmailSender sender, IDataProtectionProvider protection)
{
    private readonly IDataProtector protector = protection.CreateProtector("Koffi.MailJob.v5");
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    private static bool Shape(string? token) => token is { Length: 43 } && token.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_');
    public async Task<string> RequestAsync(int id, string culture)
    {
        if (!sender.IsConfigured || PublicOrigin.Get(config) == null)
            return "MailUnavailable";
        await using var tx = await db.Database.BeginTransactionAsync();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);
        if (user == null || user.EmailConfirmed)
            return "AlreadyVerified";
        var now = time.GetUtcNow().UtcDateTime;
        if (await db.EmailConfirmationTokens.AnyAsync(t => t.UserId == id && t.CreatedAtUtc > now.AddMinutes(-2)))
            return "VerificationThrottled";
        await db.EmailConfirmationTokens.Where(t => t.UserId == id && t.UsedAtUtc == null).ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAtUtc, now));
        await db.MailJobs.Where(j => j.UserId == id && j.Kind == "verify" && j.State == "Pending").ExecuteUpdateAsync(s => s.SetProperty(j => j.State, "Cancelled").SetProperty(j => j.ProtectedPayload, ""));
        var raw = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var hash = Hash(raw);
        db.EmailConfirmationTokens.Add(new() { UserId = id, Email = user.Email, TokenHash = hash, CreatedAtUtc = now, ExpiresAtUtc = now.AddHours(24) });
        var url = PublicOrigin.Link(config, "/Account/ConfirmEmail?token=" + raw + "&culture=" + MailingPreferenceService.Culture(culture));
        db.MailJobs.Add(new() { UserId = id, Kind = "verify", DeduplicationKey = "verify:" + hash, Culture = MailingPreferenceService.Culture(culture), ProtectedPayload = protector.Protect(url), CreatedAtUtc = now, NextAttemptUtc = now });
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return "VerificationQueued";
    }

    public async Task<bool> ValidAsync(string? raw)
    {
        if (!Shape(raw))
            return false;
        var hash = Hash(raw!);
        var now = time.GetUtcNow().UtcDateTime;
        return await db.EmailConfirmationTokens.AnyAsync(t => t.TokenHash == hash && t.UsedAtUtc == null && t.ExpiresAtUtc > now && t.Email == t.User.Email);
    }

    public async Task<bool> ConfirmAsync(string? raw)
    {
        if (!Shape(raw))
            return false;
        await using var tx = await db.Database.BeginTransactionAsync();
        var hash = Hash(raw!);
        var now = time.GetUtcNow().UtcDateTime;
        var token = await db.EmailConfirmationTokens.Include(t => t.User).SingleOrDefaultAsync(t => t.TokenHash == hash && t.UsedAtUtc == null && t.ExpiresAtUtc > now && t.Email == t.User.Email);
        if (token == null)
            return false;
        token.User.EmailConfirmed = true;
        token.UsedAtUtc = now;
        await db.SaveChangesAsync();
        await db.EmailConfirmationTokens.Where(t => t.UserId == token.UserId && t.UsedAtUtc == null).ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAtUtc, now));
        await tx.CommitAsync();
        return true;
    }
}
