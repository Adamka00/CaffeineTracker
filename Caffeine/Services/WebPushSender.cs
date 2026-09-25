using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using Caffeine.Models;
using WebPush;

namespace Caffeine.Services;

public enum PushResult
{
    Sent,
    Gone,
    Retry
}

public interface IPushSender
{
    Task<PushResult> SendAsync(
        BrowserPushSubscription subscription,
        string body,
        string url,
        string tag,
        CancellationToken token);
}

public sealed class WebPushSender(
    HttpClient http,
    PushConfiguration config)
    : IPushSender
{
    public async Task<PushResult> SendAsync(
        BrowserPushSubscription subscription,
        string body,
        string url,
        string tag,
        CancellationToken token)
    {
        if (!config.IsConfigured ||
            !PushSubscriptionService.IsAllowedEndpoint(
                subscription.Endpoint))
        {
            return PushResult.Gone;
        }

        var client =
            new WebPushClient(http);

        var payload =
            JsonSerializer.Serialize(
                new
                {
                    title = "Koffi",
                    body,
                    url,
                    tag
                });

        try
        {
            await client.SendNotificationAsync(
                new PushSubscription(
                    subscription.Endpoint,
                    subscription.P256dh,
                    subscription.Auth),

                payload,

                new Dictionary<string, object>
                {
                    ["vapidDetails"] =
                        new VapidDetails(
                            config.Subject,
                            config.PublicKey,
                            config.PrivateKey),

                    ["TTL"] = 1800
                },

                token);

            return PushResult.Sent;
        }
        catch (WebPushException ex)
            when (
                ex.StatusCode is
                    HttpStatusCode.Gone or
                    HttpStatusCode.NotFound)
        {
            return PushResult.Gone;
        }
        catch (OperationCanceledException)
            when (!token.IsCancellationRequested)
        {
            return PushResult.Retry;
        }
        catch (Exception ex)
            when (
                ex is WebPushException
                    or HttpRequestException
                    or CryptographicException)
        {
            return PushResult.Retry;
        }
    }
}