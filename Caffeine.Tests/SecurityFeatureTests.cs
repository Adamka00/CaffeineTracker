using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Caffeine.Tests;

public sealed class KoffiTestClock : TimeProvider
{
    public DateTimeOffset At { get; set; } =
        new(
            2026,
            9,
            17,
            12,
            0,
            0,
            TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() =>
        At;
}

public sealed class CapturingEmail : IEmailSender
{
    public bool IsConfigured => true;

    public ConcurrentQueue<ResetMail> Messages { get; } =
        new();

    public Task SendResetAsync(
        ResetMail mail,
        CancellationToken token)
    {
        Messages.Enqueue(mail);

        return Task.CompletedTask;
    }
}

public sealed class CapturingPush : IPushSender
{
    public List<(int Id, string Body, string Tag)> Messages { get; } =
        [];

    public PushResult Result { get; set; } =
        PushResult.Sent;

    public Task<PushResult> SendAsync(
        BrowserPushSubscription sub,
        string body,
        string url,
        string tag,
        CancellationToken token)
    {
        Messages.Add(
            (
                sub.Id,
                body,
                tag
            ));

        return Task.FromResult(Result);
    }
}

public sealed class KoffiFeatureApp : WebApplicationFactory<Program>
{
    private readonly string path =
        Path.Combine(
            Path.GetTempPath(),
            $"koffi3-{Guid.NewGuid():N}.db");

    public KoffiTestClock Clock { get; } =
        new();

    public CapturingEmail Email { get; } =
        new();

    public CapturingPush Push { get; } =
        new();

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        var keys =
            WebPush.VapidHelper
                .GenerateVapidKeys();

        builder.ConfigureAppConfiguration(
            (_, config) =>
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Account:PublicOrigin"] =
                            "https://koffi.example",

                        ["Push:Enabled"] =
                            "true",

                        ["Push:PublicKey"] =
                            keys.PublicKey,

                        ["Push:PrivateKey"] =
                            keys.PrivateKey,

                        ["Push:Subject"] =
                            "mailto:test@example.test"
                    }));

        builder.ConfigureServices(
            services =>
            {
                services.RemoveAll<
                    DbContextOptions<AppDbContext>>();

                services.RemoveAll<
                    IDbContextOptionsConfiguration<AppDbContext>>();

                services.AddDbContext<AppDbContext>(
                    options =>
                        options.UseSqlite(
                            $"Data Source={path}"));

                services.RemoveAll<TimeProvider>();

                services.AddSingleton<TimeProvider>(
                    Clock);

                services.RemoveAll<IEmailSender>();

                services.AddSingleton<IEmailSender>(
                    Email);

                services.RemoveAll<IPushSender>();

                services.AddSingleton<IPushSender>(
                    Push);

                // Scheduling is driven manually in tests;
                // real email/push networks are never used.
                foreach (
                    var descriptor in services
                        .Where(
                            d =>
                                d.ServiceType ==
                                    typeof(IHostedService) &&
                                d.ImplementationType ==
                                    typeof(NotificationWorker))
                        .ToList())
                {
                    services.Remove(descriptor);
                }
            });
    }

    protected override void Dispose(
        bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            Microsoft.Data.Sqlite
                .SqliteConnection
                .ClearAllPools();

            File.Delete(path);
        }
    }
}

public sealed class SecurityFeatureTests : IDisposable
{
    private readonly KoffiFeatureApp app =
        new();

    private readonly HttpClient client;

    public SecurityFeatureTests()
    {
        client =
            app.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

        using var scope =
            app.Services.CreateScope();

        DatabaseUpgrade.ApplyAsync(
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>())
            .GetAwaiter()
            .GetResult();
    }

    private async Task<T> Work<T>(
        Func<
            IServiceProvider,
            AppDbContext,
            Task<T>> work)
    {
        using var scope =
            app.Services.CreateScope();

        return await work(
            scope.ServiceProvider,
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>());
    }

    private static async Task<HttpResponseMessage> Post(
        HttpClient browser,
        string action,
        Dictionary<string, string> data,
        string page = "/")
    {
        var html =
            await browser
                .GetStringAsync(page);

        var match =
            Regex.Match(
                html,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");

        Assert.True(match.Success);

        data["__RequestVerificationToken"] =
            WebUtility.HtmlDecode(
                match.Groups[1].Value);

        return await browser.PostAsync(
            action,
            new FormUrlEncodedContent(data));
    }

    private Task<int> AddUser() =>
        Work(
            async (_, db) =>
            {
                var user =
                    new AppUser
                    {
                        Username =
                            "resetuser",

                        Email =
                            "reset@example.test"
                    };

                user.PasswordHash =
                    new PasswordHasher<AppUser>()
                        .HashPassword(
                            user,
                            "Old-password123");

                db.Users.Add(user);

                await db.SaveChangesAsync();

                return user.Id;
            });

    private async Task<string> RequestToken()
    {
        await Work(
            async (services, _) =>
            {
                await services
                    .GetRequiredService<
                        PasswordResetService>()
                    .RequestAsync(
                        "reset@example.test",
                        "en");

                return 0;
            });

        for (var i = 0; i < 100; i++)
        {
            if (
                app.Email.Messages
                    .TryDequeue(
                        out var mail))
            {
                return QueryHelpers
                    .ParseQuery(
                        new Uri(mail.Url).Query)
                    ["token"]
                    .ToString();
            }

            await Task.Delay(10);
        }

        throw new InvalidOperationException(
            "No test email received.");
    }

    private static PushForm PushData(
        string name = "device")
    {
        using var key =
            ECDiffieHellman.Create(
                ECCurve.NamedCurves.nistP256);

        var p =
            key.ExportParameters(false);

        return new()
        {
            Endpoint =
                "https://fcm.googleapis.com/fcm/send/" +
                name,

            P256dh =
                WebEncoders.Base64UrlEncode(
                    new byte[] { 4 }
                        .Concat(p.Q.X!)
                        .Concat(p.Q.Y!)
                        .ToArray()),

            Auth =
                WebEncoders.Base64UrlEncode(
                    RandomNumberGenerator
                        .GetBytes(16))
        };
    }

    [Fact]
    public async Task ResetStoresOnlyHashIsSingleUseAndActuallyChangesPassword()
    {
        var id =
            await AddUser();

        var token =
            await RequestToken();

        Assert.Equal(
            43,
            token.Length);

        await Work(
            async (services, db) =>
            {
                var row =
                    await db
                        .PasswordResetTokens
                        .SingleAsync();

                Assert.NotEqual(
                    token,
                    row.TokenHash);

                Assert.Equal(
                    64,
                    row.TokenHash.Length);

                Assert.Equal(
                    TimeSpan.FromMinutes(30),
                    row.ExpiresAt -
                    row.CreatedAt);

                var reset =
                    services
                        .GetRequiredService<
                            PasswordResetService>();

                Assert.False(
                    await reset.ResetAsync(
                        "not-a-token",
                        "New-password123"));

                Assert.True(
                    await reset.ResetAsync(
                        token,
                        "New-password123"));

                Assert.False(
                    await reset.ResetAsync(
                        token,
                        "Another-password123"));

                var user =
                    await db.Users
                        .SingleAsync(
                            u =>
                                u.Id == id);

                var hasher =
                    new PasswordHasher<AppUser>();

                Assert.Equal(
                    PasswordVerificationResult.Success,
                    hasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        "New-password123"));

                Assert.Equal(
                    PasswordVerificationResult.Failed,
                    hasher.VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        "Old-password123"));

                Assert.NotEmpty(
                    user.SecurityStamp);

                return 0;
            });
    }

    [Fact]
    public async Task ExpiryAndReplacementTokensAreEnforced()
    {
        await AddUser();

        var first =
            await RequestToken();

        app.Clock.At =
            app.Clock.At
                .AddMinutes(2);

        var second =
            await RequestToken();

        await Work(
            async (s, _) =>
            {
                Assert.False(
                    await s
                        .GetRequiredService<
                            PasswordResetService>()
                        .IsValidAsync(first));

                return 0;
            });

        app.Clock.At =
            app.Clock.At
                .AddMinutes(30);

        await Work(
            async (s, _) =>
            {
                Assert.False(
                    await s
                        .GetRequiredService<
                            PasswordResetService>()
                        .ResetAsync(
                            second,
                            "New-password123"));

                return 0;
            });
    }

    [Fact]
    public async Task ForgotResponsesDoNotRevealAccountAndRequestsAreRateLimited()
    {
        await AddUser();

        var exists =
            await Post(
                client,
                "/Account/ForgotPassword?culture=en",
                new()
                {
                    ["Email"] =
                        "reset@example.test"
                },
                "/Account/ForgotPassword?culture=en");

        var missing =
            await Post(
                client,
                "/Account/ForgotPassword?culture=en",
                new()
                {
                    ["Email"] =
                        "missing@example.test"
                },
                "/Account/ForgotPassword?culture=en");

        Assert.Equal(
            exists.StatusCode,
            missing.StatusCode);

        var a =
            WebUtility.HtmlDecode(
                await exists.Content
                    .ReadAsStringAsync());

        var b =
            WebUtility.HtmlDecode(
                await missing.Content
                    .ReadAsStringAsync());

        Assert.Contains(
            "If an account exists",
            a);

        Assert.Contains(
            "If an account exists",
            b);

        Assert.DoesNotContain(
            "reset@example.test",
            a);

        Assert.DoesNotContain(
            "missing@example.test",
            b);

        Assert.DoesNotContain(
            "cdn.",
            a);

        await Post(
            client,
            "/Account/ForgotPassword?culture=en",
            new()
            {
                ["Email"] =
                    "reset@example.test"
            },
            "/Account/ForgotPassword");

        Assert.Equal(
            1,
            await Work(
                (_, db) =>
                    db.PasswordResetTokens
                        .CountAsync()));

        for (var i = 0; i < 2; i++)
        {
            await Post(
                client,
                "/Account/ForgotPassword?culture=en",
                new()
                {
                    ["Email"] =
                        "missing@example.test"
                },
                "/Account/ForgotPassword");
        }

        Assert.Equal(
            HttpStatusCode.TooManyRequests,
            (
                await Post(
                    client,
                    "/Account/ForgotPassword?culture=en",
                    new()
                    {
                        ["Email"] =
                            "missing@example.test"
                    },
                    "/Account/ForgotPassword")
            ).StatusCode);
    }

    [Fact]
    public async Task ResetInvalidatesExistingLoginAndRecoveryHasNoThirdPartyScripts()
    {
        await AddUser();

        await Post(
            client,
            "/Account/Login",
            new()
            {
                ["Email"] =
                    "reset@example.test",

                ["Password"] =
                    "Old-password123"
            },
            "/Account/Login");

        Assert.Contains(
            "resetuser",
            await client
                .GetStringAsync("/"));

        var token =
            await RequestToken();

        var page =
            "/Account/ResetPassword?token=" +
            token;

        var response =
            await client
                .GetAsync(page);

        var html =
            await response.Content
                .ReadAsStringAsync();

        Assert.DoesNotContain(
            "<script",
            html);

        Assert.Contains(
            "no-referrer",
            response.Headers
                .GetValues(
                    "Referrer-Policy"));

        await Post(
            client,
            "/Account/ResetPassword",
            new()
            {
                ["Token"] =
                    token,

                ["Password"] =
                    "New-password123",

                ["ConfirmPassword"] =
                    "New-password123"
            },
            page);

        Assert.DoesNotContain(
            "resetuser",
            await client
                .GetStringAsync("/"));
    }

    [Fact]
    public async Task ConcurrentResetOnlyConsumesTokenOnce()
    {
        await AddUser();

        var token =
            await RequestToken();

        var results =
            await Task.WhenAll(
                Enumerable
                    .Range(0, 2)
                    .Select(
                        _ =>
                            Work(
                                (s, db) =>
                                    s
                                        .GetRequiredService<
                                            PasswordResetService>()
                                        .ResetAsync(
                                            token,
                                            "New-password123"))));

        Assert.Single(
            results,
            x => x);
    }

    [Theory]
    [InlineData("http://fcm.googleapis.com/send/a")]
    [InlineData("https://127.0.0.1/a")]
    [InlineData("https://fcm.googleapis.com.evil.test/a")]
    [InlineData("https://evil.test/a")]
    [InlineData("https://fcm.googleapis.com:444/a")]
    public void PushRejectsUntrustedEndpoints(
        string endpoint) =>
        Assert.False(
            PushSubscriptionService
                .IsAllowedEndpoint(endpoint));

    [Fact]
    public async Task PushSubscriptionsAreIdempotentAndCannotBeReadoptedOrDeletedByAnotherUser()
    {
        var form =
            PushData();

        await Work(
            async (s, db) =>
            {
                var service =
                    s.GetRequiredService<
                        PushSubscriptionService>();

                Assert.True(
                    await service
                        .SubscribeAsync(
                            "owner",
                            form));

                Assert.True(
                    await service
                        .SubscribeAsync(
                            "owner",
                            form));

                Assert.False(
                    await service
                        .SubscribeAsync(
                            "outsider",
                            form));

                await service
                    .UnsubscribeAsync(
                        "outsider",
                        form.Endpoint);

                Assert.Single(
                    await db
                        .BrowserPushSubscriptions
                        .ToListAsync());

                await service
                    .UnsubscribeAsync(
                        "owner",
                        form.Endpoint);

                Assert.Empty(
                    await db
                        .BrowserPushSubscriptions
                        .ToListAsync());

                return 0;
            });
    }

    [Fact]
    public async Task PushEndpointsEnforceCsrfAndOwnership()
    {
        var form =
            PushData();

        var data =
            new Dictionary<string, string>
            {
                ["Endpoint"] =
                    form.Endpoint,

                ["P256dh"] =
                    form.P256dh,

                ["Auth"] =
                    form.Auth
            };

        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await client.PostAsync(
                    "/Notifications/Subscribe",
                    new FormUrlEncodedContent(
                        data))
            ).StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            (
                await Post(
                    client,
                    "/Notifications/Subscribe",
                    data)
            ).StatusCode);

        using var outsider =
            app.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

        await Post(
            outsider,
            "/Notifications/Unsubscribe",
            new()
            {
                ["endpoint"] =
                    form.Endpoint
            });

        Assert.Equal(
            1,
            await Work(
                (_, db) =>
                    db
                        .BrowserPushSubscriptions
                        .CountAsync()));

        Assert.Equal(
            HttpStatusCode.Conflict,
            (
                await Post(
                    outsider,
                    "/Notifications/Subscribe",
                    data)
            ).StatusCode);

        await Post(
            client,
            "/Notifications/Unsubscribe",
            new()
            {
                ["endpoint"] =
                    form.Endpoint
            });

        Assert.Equal(
            0,
            await Work(
                (_, db) =>
                    db
                        .BrowserPushSubscriptions
                        .CountAsync()));
    }

    [Fact]
    public async Task SchedulerHonorsPreferencesCompletedDaysAndPersistentDeduplication()
    {
        await Work(
            async (s, db) =>
            {
                await s
                    .GetRequiredService<
                        PushSubscriptionService>()
                    .SubscribeAsync(
                        "owner",
                        PushData());

                db.NotificationPreferences.Add(
                    new()
                    {
                        UserId =
                            "owner",

                        StreakReminder =
                            true,

                        StreakTime =
                            "14:00",

                        Culture =
                            "en"
                    });

                await db.SaveChangesAsync();

                var notifications =
                    s.GetRequiredService<
                        NotificationService>();

                await notifications
                    .RunOnceAsync(default);

                await notifications
                    .RunOnceAsync(default);

                Assert.Single(
                    app.Push.Messages);

                Assert.Contains(
                    "no caffeine needed",
                    app.Push
                        .Messages[0]
                        .Body);

                Assert.Single(
                    await db
                        .NotificationDeliveries
                        .Where(
                            d =>
                                d.SentAtUtc != null)
                        .ToListAsync());

                return 0;
            });

        // New scope represents a process cycle/restart;
        // database dedup survives it.
        await Work(
            async (s, _) =>
            {
                await s
                    .GetRequiredService<
                        NotificationService>()
                    .RunOnceAsync(default);

                return 0;
            });

        Assert.Single(
            app.Push.Messages);

        app.Clock.At =
            app.Clock.At
                .AddDays(1);

        await Work(
            async (s, db) =>
            {
                db.CaffeineFreeDays.Add(
                    new()
                    {
                        UserId =
                            "owner",

                        Day =
                            new DateTime(
                                2026,
                                9,
                                18)
                    });

                await db.SaveChangesAsync();

                await s
                    .GetRequiredService<
                        NotificationService>()
                    .RunOnceAsync(default);

                return 0;
            });

        Assert.Single(
            app.Push.Messages);
    }

    [Fact]
    public async Task SchedulerRemovesGoneSubscriptionsAndRetriesTransientFailures()
    {
        await Work(
            async (s, db) =>
            {
                await s
                    .GetRequiredService<
                        PushSubscriptionService>()
                    .SubscribeAsync(
                        "owner",
                        PushData());

                db.NotificationPreferences.Add(
                    new()
                    {
                        UserId =
                            "owner",

                        SleepCheckIn =
                            true,

                        MorningTime =
                            "14:00"
                    });

                await db.SaveChangesAsync();

                app.Push.Result =
                    PushResult.Retry;

                await s
                    .GetRequiredService<
                        NotificationService>()
                    .RunOnceAsync(default);

                await s
                    .GetRequiredService<
                        NotificationService>()
                    .RunOnceAsync(default);

                Assert.Single(
                    app.Push.Messages);

                return 0;
            });

        app.Clock.At =
            app.Clock.At
                .AddMinutes(2);

        app.Push.Result =
            PushResult.Gone;

        await Work(
            async (s, db) =>
            {
                await s
                    .GetRequiredService<
                        NotificationService>()
                    .RunOnceAsync(default);

                Assert.Empty(
                    await db
                        .BrowserPushSubscriptions
                        .ToListAsync());

                Assert.Empty(
                    await db
                        .NotificationDeliveries
                        .ToListAsync());

                return 0;
            });

        Assert.Equal(
            2,
            app.Push.Messages.Count);
    }

    [Fact]
    public async Task SchedulerFindsCutoffThresholdAndMorningButSuppressesCompletedSleep()
    {
        await Work(
            async (services, db) =>
            {
                var clock =
                    services
                        .GetRequiredService<
                            TrackerClock>();

                var planning =
                    services
                        .GetRequiredService<
                            CaffeinePlanningService>();

                var notification =
                    services
                        .GetRequiredService<
                            NotificationService>();

                var now =
                    clock.Now;

                var bedtime =
                    now
                        .AddHours(10.75)
                        .AddMinutes(10);

                db.TrackerPreferences.Add(
                    new()
                    {
                        UserId =
                            "cutoff-user",

                        PlannedBedtime =
                            bedtime.ToString(
                                "HH:mm"),

                        TargetMg =
                            20
                    });

                await db.SaveChangesAsync();

                var cutoff =
                    await notification
                        .DueAsync(
                            new()
                            {
                                UserId =
                                    "cutoff-user",

                                CutoffReminder =
                                    true,

                                CutoffDoseMg =
                                    80
                            },
                            now);

                Assert.Single(
                    cutoff,
                    d =>
                        d.Kind ==
                        "cutoff");

                var consumed =
                    now.AddHours(-14.75);

                db.CaffeineLogs.Add(
                    new()
                    {
                        UserId =
                            "threshold-user",

                        BeverageId =
                            3,

                        TotalCaffeineMg =
                            160,

                        ConsumedAt =
                            consumed,

                        ConsumedAmountMl =
                            500
                    });

                await db.SaveChangesAsync();

                var logs =
                    await db
                        .CaffeineLogs
                        .Where(
                            l =>
                                l.UserId ==
                                "threshold-user")
                        .ToListAsync();

                var crossing =
                    planning
                        .UnderTarget(
                            logs,
                            consumed,
                            25)!
                        .Value;

                var threshold =
                    await notification
                        .DueAsync(
                            new()
                            {
                                UserId =
                                    "threshold-user",

                                ThresholdReached =
                                    true
                            },
                            crossing
                                .AddMinutes(1));

                Assert.Single(
                    threshold,
                    d =>
                        d.Kind ==
                        "threshold");

                var morning =
                    new NotificationPreference
                    {
                        UserId =
                            "morning-user",

                        SleepCheckIn =
                            true,

                        MorningTime =
                            now.ToString(
                                "HH:mm")
                    };

                Assert.Single(
                    await notification
                        .DueAsync(
                            morning,
                            now),
                    d =>
                        d.Kind ==
                        "sleep");

                db.SleepLogs.Add(
                    new()
                    {
                        UserId =
                            "morning-user",

                        SleepDate =
                            now.Date,

                        ActualBedtime =
                            now.AddHours(-8),

                        WakeTime =
                            now,

                        SleepRating =
                            3
                    });

                await db.SaveChangesAsync();

                Assert.Empty(
                    await notification
                        .DueAsync(
                            morning,
                            now));

                Assert.Empty(
                    await notification
                        .DueAsync(
                            new()
                            {
                                UserId =
                                    "morning-user"
                            },
                            now));

                return 0;
            });
    }

    public void Dispose()
    {
        client.Dispose();
        app.Dispose();
    }
}