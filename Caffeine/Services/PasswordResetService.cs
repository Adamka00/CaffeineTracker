using System.Security.Cryptography;
using System.Text;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed class PasswordResetService(
    AppDbContext db,
    TimeProvider time,
    ResetMailQueue queue,
    IEmailSender sender,
    IConfiguration config)
{
    private static string Hash(string token) =>
        Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(token)));

    private static bool ValidShape(string? token) =>
        token is { Length: 43 } &&
        token.All(c =>
            char.IsAsciiLetterOrDigit(c) ||
            c is '-' or '_');

    public async Task RequestAsync(
        string email,
        string culture)
    {
        var origin =
            config["Account:PublicOrigin"];

        if (!sender.IsConfigured ||
            !Uri.TryCreate(
                origin,
                UriKind.Absolute,
                out var uri) ||
            uri.Scheme != "https" ||
            uri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment) ||
            !string.IsNullOrEmpty(uri.UserInfo))
        {
            return;
        }

        var normalized =
            email.Trim().ToLowerInvariant();

        var matches =
            await db.Users
                .Where(u =>
                    u.Email.ToLower() == normalized)
                .Take(2)
                .ToListAsync();

        if (matches.Count != 1)
            return;

        var user = matches[0];

        var now =
            time.GetUtcNow().UtcDateTime;

        await db.PasswordResetTokens
            .Where(t =>
                t.ExpiresAt < now.AddDays(-1))
            .ExecuteDeleteAsync();

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        if (await db.PasswordResetTokens.AnyAsync(
                t =>
                    t.UserId == user.Id &&
                    t.CreatedAt > now.AddMinutes(-1)))
        {
            return;
        }

        await db.PasswordResetTokens
            .Where(t =>
                t.UserId == user.Id &&
                t.UsedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    t => t.UsedAt,
                    now));

        var raw =
            WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(32));

        var record =
            new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = Hash(raw),
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(30)
            };

        db.PasswordResetTokens.Add(record);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        var url =
            uri.GetLeftPart(UriPartial.Authority) +
            "/Account/ResetPassword?token=" +
            raw +
            "&culture=" +
            (culture == "hu" ? "hu" : "en");

        if (!queue.TryQueue(
                new(
                    user.Email,
                    url,
                    culture)))
        {
            await db.PasswordResetTokens
                .Where(t =>
                    t.Id == record.Id)
                .ExecuteDeleteAsync();
        }
    }

    public async Task<bool> IsValidAsync(
        string? token)
    {
        if (!ValidShape(token))
            return false;

        var hash =
            Hash(token!);

        var now =
            time.GetUtcNow().UtcDateTime;

        return await db.PasswordResetTokens
            .AnyAsync(t =>
                t.TokenHash == hash &&
                t.UsedAt == null &&
                t.ExpiresAt > now);
    }

    public async Task<bool> ResetAsync(
        string? token,
        string password)
    {
        if (!ValidShape(token) ||
            password.Length is < 10 or > 128)
        {
            return false;
        }

        var hash =
            Hash(token!);

        var now =
            time.GetUtcNow().UtcDateTime;

        await db.PasswordResetTokens
            .Where(t =>
                t.ExpiresAt < now.AddDays(-1))
            .ExecuteDeleteAsync();

        await using var transaction =
            await db.Database.BeginTransactionAsync();

        var record =
            await db.PasswordResetTokens
                .Include(t => t.User)
                .SingleOrDefaultAsync(t =>
                    t.TokenHash == hash &&
                    t.UsedAt == null &&
                    t.ExpiresAt > now);

        if (record == null)
            return false;

        record.User.PasswordHash =
            new PasswordHasher<AppUser>()
                .HashPassword(
                    record.User,
                    password);

        record.User.SecurityStamp =
            Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

        record.UsedAt = now;

        await db.SaveChangesAsync();

        await db.PasswordResetTokens
            .Where(t =>
                t.UserId == record.UserId &&
                t.UsedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(
                    t => t.UsedAt,
                    now));

        await transaction.CommitAsync();

        return true;
    }
}