using System.Security.Cryptography;
using System.Text;
using Caffeine.Data;
using Caffeine.Models;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace Caffeine.Services;

public sealed class PushConfiguration(
    IConfiguration configuration)
{
    public string PublicKey =>
        configuration["Push:PublicKey"] ?? "";

    public string PrivateKey =>
        configuration["Push:PrivateKey"] ?? "";

    public string Subject =>
        configuration["Push:Subject"] ?? "";

    public bool Enabled =>
        configuration.GetValue<bool>("Push:Enabled");


    public bool IsConfigured =>
        Enabled &&
        KeyLength(PublicKey, 65) &&
        KeyLength(PrivateKey, 32) &&
        Uri.TryCreate(
            Subject,
            UriKind.Absolute,
            out var subject) &&
        subject.Scheme is "mailto" or "https";


    private static bool KeyLength(
        string value,
        int length)
    {
        try
        {
            return WebEncoders
                .Base64UrlDecode(value)
                .Length == length;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}


public sealed class PushSubscriptionService(
    AppDbContext db,
    TimeProvider time)
{
    public static string HashEndpoint(
        string endpoint) =>
        Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(endpoint)));


    public static bool IsAllowedEndpoint(
        string endpoint)
    {
        if (!Uri.TryCreate(
                endpoint,
                UriKind.Absolute,
                out var uri) ||
            uri.Scheme != "https" ||
            uri.Port != 443 ||
            uri.UserInfo.Length != 0 ||
            uri.Fragment.Length != 0 ||
            endpoint.Length > 2048)
        {
            return false;
        }


        var h =
            uri.IdnHost;


        return
            h == "fcm.googleapis.com" ||
            h == "web.push.apple.com" ||
            h == "updates.push.services.mozilla.com" ||

            h.EndsWith(
                ".push.services.mozilla.com",
                StringComparison.Ordinal) ||

            h.EndsWith(
                ".notify.windows.com",
                StringComparison.Ordinal);
    }


    public static bool Valid(
        PushForm form)
    {
        if (!IsAllowedEndpoint(
                form.Endpoint))
        {
            return false;
        }


        try
        {
            var key =
                WebEncoders.Base64UrlDecode(
                    form.P256dh);


            if (key.Length != 65 ||
                key[0] != 4 ||
                WebEncoders
                    .Base64UrlDecode(
                        form.Auth)
                    .Length != 16)
            {
                return false;
            }


            using var ec =
                ECDiffieHellman.Create(
                    new ECParameters
                    {
                        Curve =
                            ECCurve.NamedCurves
                                .nistP256,

                        Q =
                            new ECPoint
                            {
                                X = key[1..33],
                                Y = key[33..65]
                            }
                    });


            return true;
        }
        catch (Exception ex)
            when (
                ex is FormatException
                    or CryptographicException
                    or ArgumentException)
        {
            return false;
        }
    }


    public async Task<bool> SubscribeAsync(
        string userId,
        PushForm form)
    {
        if (!Valid(form))
            return false;


        await using var transaction =
            await db.Database
                .BeginTransactionAsync();


        var hash =
            HashEndpoint(form.Endpoint);


        var entry =
            await db.BrowserPushSubscriptions
                .SingleOrDefaultAsync(
                    s =>
                        s.EndpointHash ==
                        hash);


        if (entry != null &&
            entry.UserId != userId)
        {
            return false;
        }


        if (entry == null)
        {
            if (await db.BrowserPushSubscriptions
                    .CountAsync(
                        s =>
                            s.UserId ==
                            userId)
                >= 10)
            {
                return false;
            }


            entry =
                new BrowserPushSubscription
                {
                    UserId =
                        userId,

                    EndpointHash =
                        hash,

                    CreatedAtUtc =
                        time
                            .GetUtcNow()
                            .UtcDateTime
                };


            db.BrowserPushSubscriptions.Add(
                entry);
        }


        entry.Endpoint =
            form.Endpoint;

        entry.Auth =
            form.Auth;

        entry.P256dh =
            form.P256dh;

        entry.LastSeenUtc =
            time
                .GetUtcNow()
                .UtcDateTime;


        await db.SaveChangesAsync();

        await transaction.CommitAsync();


        return true;
    }


    public Task<int> UnsubscribeAsync(
        string userId,
        string endpoint) =>
        db.BrowserPushSubscriptions
            .Where(s =>
                s.UserId == userId &&
                s.EndpointHash ==
                HashEndpoint(endpoint))
            .ExecuteDeleteAsync();
}