using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Services;
using Caffeine.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Caffeine.Tests;
public sealed class Koffi50PolicyTests
{
    [Fact]
    public void OptionalSleepValuesAreNotImputedAsZero()
    {
        var start = new DateTime(2026, 10, 5);
        var rows = new[]
        {
            new SleepLog
            {
                SleepDate = start,
                ActualBedtime = start.AddHours(-8),
                WakeTime = start,
                SleepRating = 4
            },
            new SleepLog
            {
                SleepDate = start.AddDays(1),
                ActualBedtime = start.AddDays(1).AddHours(-7.5),
                WakeTime = start.AddDays(1),
                SleepRating = 2,
                Awakenings = 0,
                FallingAsleep = 5,
                PlannedBedtime = start.AddDays(1).AddHours(-8)
            }
        };
        var s = SleepStatisticsService.Calculate(start, rows);
        Assert.Equal(2, s.Count);
        Assert.Equal(7.75, s.AverageHours);
        Assert.Equal(0, s.AverageAwakenings);
        Assert.Equal(1, s.AwakeningCount);
        Assert.Equal(5, s.AverageDifficulty);
        Assert.Equal(30, s.PlannedDifferenceMinutes);
        Assert.Equal(5, s.Days.Count(d => d.Hours == null));
        Assert.Null(SleepStatisticsService.Calculate(start, []).AverageHours);
    }

    [Theory]
    [InlineData(2026, 10, 11, 2026, 10, 5)]
    [InlineData(2026, 10, 12, 2026, 10, 12)]
    public void WeeksBeginOnMonday(int y, int m, int d, int ey, int em, int ed) => Assert.Equal(new DateTime(ey, em, ed), SleepStatisticsService.Monday(new(y, m, d)));
    [Theory]
    [InlineData(2026, 10, 12, 7, 2026, 9, 28)]
    [InlineData(2026, 10, 12, 8, 2026, 10, 5)]
    [InlineData(2026, 10, 11, 23, 2026, 9, 28)]
    public void WeeklyScheduleOnlyUsesCompletedWeeks(int y, int m, int d, int h, int ey, int em, int ed) => Assert.Equal(new DateTime(ey, em, ed), WeeklySummaryService.LatestDueWeek(new(y, m, d, h, 0, 0), 1, TimeSpan.FromHours(8)));
    [Fact]
    public void DstClockRejectsNonexistentTimeAndRequiresAmbiguousOffset()
    {
        var config = new ConfigurationBuilder().Build();
        var clock = new TrackerClock(config, TimeProvider.System);
        Assert.False(clock.TryUtc(new DateTime(2026, 3, 29, 2, 30, 0), null, out _));
        Assert.False(clock.TryUtc(new DateTime(2026, 10, 25, 2, 30, 0), null, out _));
        Assert.True(clock.TryUtc(new DateTime(2026, 10, 25, 2, 30, 0), 120, out var summer));
        Assert.True(clock.TryUtc(new DateTime(2026, 10, 25, 2, 30, 0), 60, out var standard));
        Assert.Equal(TimeSpan.FromHours(1), standard - summer);
    }

    [Theory]
    [InlineData(false, 0, 0, false)]
    [InlineData(false, 10, 1, true)]
    [InlineData(false, 1, 5, true)]
    [InlineData(true, 100, 100, false)]
    public void GuestPromptIsOptionalAndNeverForAccounts(bool auth, int drinks, int days, bool show) => Assert.Equal(show, GuestPromptService.Due(auth, drinks, days, null, new DateTime(2026, 10, 5)));
    [Fact]
    public void GuestDismissalHasCalendarMemory() => Assert.False(GuestPromptService.Due(false, 100, 10, new DateTime(2026, 10, 20), new DateTime(2026, 10, 5)));
    [Fact]
    public void MetricSpecificObservationUsesActualDirection()
    {
        var lower = new SleepInsight("BedtimeCaffeineVsRating", "SleepLowerAssociation", 20, -.5);
        Assert.Equal("Observation_BedtimeCaffeineVsRating_Lower", lower.ObservationKey);
        Assert.Equal("SleepObservationUnclear", new SleepInsight("BedtimeCaffeineVsRating", "SleepNoClearAssociation", 30, .1).ObservationKey);
        Assert.Equal("InsufficientSleepData", new SleepInsight("BedtimeCaffeineVsRating", "InsufficientSleepData", 3, 0).ObservationKey);
    }
}

public sealed class Koffi50IntegrationTests : IDisposable
{
    private readonly KoffiFeatureApp app = new();
    private readonly HttpClient client;
    public Koffi50IntegrationTests()
    {
        client = app.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        using var scope = app.Services.CreateScope();
        DatabaseUpgrade.ApplyAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>()).GetAwaiter().GetResult();
    }

    private async Task<T> Work<T>(Func<IServiceProvider, AppDbContext, Task<T>> action)
    {
        using var scope = app.Services.CreateScope();
        return await action(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    private async Task<int> User(string email = "person@example.test", bool confirmed = true) => await Work(async (_, db) =>
    {
        var u = new AppUser
        {
            Username = "person",
            Email = email,
            PasswordHash = "original-hash",
            EmailConfirmed = confirmed
        };
        db.Users.Add(u);
        await db.SaveChangesAsync();
        return u.Id;
    });
    private static async Task<HttpResponseMessage> Post(HttpClient c, string path, Dictionary<string, string> data, string page = "/")
    {
        var html = await c.GetStringAsync(page);
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.True(match.Success);
        data["__RequestVerificationToken"] = WebUtility.HtmlDecode(match.Groups[1].Value);
        return await c.PostAsync(path, new FormUrlEncodedContent(data));
    }

    [Theory]
    [InlineData("hu")]
    [InlineData("en")]
    [InlineData("de")]
    public async Task NewPagesUseExistingDesignAndLanguages(string culture)
    {
        foreach (var page in new[]
        {
            "/Sleep",
            "/Sleep/Statistics",
            "/Sleep/Analysis",
            "/Drinks"
        }

        )
        {
            var html = await client.GetStringAsync(page + "?culture=" + culture);
            Assert.Contains("koffi4.css", html);
            Assert.Contains("add-drink-fab", html);
            Assert.Contains("5.0", html);
        }
    }

    [Fact]
    public async Task OwnDrinkEditAndArchiveNeverChangeCaffeineHistory()
    {
        await Work(async (s, db) =>
        {
            var b = new Beverage
            {
                OwnerId = "owner",
                Name = "Own",
                Category = "Custom",
                CaffeinePer100Ml = 32,
                DefaultPortionMl = 500
            };
            db.Beverages.Add(b);
            await db.SaveChangesAsync();
            db.CaffeineLogs.Add(new CaffeineLog { UserId = "owner", BeverageId = b.Id, ConsumedAt = new DateTime(2026, 9, 1), ConsumedAmountMl = 500, TotalCaffeineMg = 160 });
            db.FavoriteDrinks.Add(new() { UserId = "owner", BeverageId = b.Id, AmountMl = 500 });
            await db.SaveChangesAsync();
            var service = s.GetRequiredService<OwnDrinkService>();
            var form = new OwnDrinkForm
            {
                Id = b.Id,
                Name = "Changed",
                Category = "Tea",
                CaffeinePer100Ml = 36,
                AmountMl = 330
            };
            Assert.Equal("NotFound", await service.UpdateAsync("outsider", form));
            Assert.Null(await service.UpdateAsync("owner", form));
            Assert.False(await service.ArchiveAsync("outsider", b.Id));
            Assert.True(await service.ArchiveAsync("owner", b.Id));
            db.ChangeTracker.Clear();
            var log = await db.CaffeineLogs.SingleAsync();
            Assert.Equal(160, log.TotalCaffeineMg);
            Assert.Equal(500, log.ConsumedAmountMl);
            Assert.True((await db.Beverages.SingleAsync(b => b.Id == form.Id)).IsArchived);
            Assert.Empty(await db.FavoriteDrinks.ToListAsync());
            Assert.DoesNotContain(await s.GetRequiredService<DrinkCatalog>().ListAsync("owner"), d => d.Id == form.Id);
            return 0;
        });
    }

    [Fact]
    public async Task WeeklySleepAndEmailStatisticsAreUserScoped()
    {
        await Work(async (s, db) =>
        {
            var start = new DateTime(2026, 9, 7);
            db.SleepLogs.AddRange(new SleepLog { UserId = "1", SleepDate = start, ActualBedtime = start.AddHours(-7.5), WakeTime = start, SleepRating = 4, Awakenings = 2 }, new SleepLog { UserId = "2", SleepDate = start, ActualBedtime = start.AddHours(-20), WakeTime = start, SleepRating = 1 });
            db.CaffeineLogs.AddRange(new CaffeineLog { UserId = "1", BeverageId = 4, ConsumedAt = start.AddHours(12), ConsumedAmountMl = 500, TotalCaffeineMg = 160 }, new CaffeineLog { UserId = "2", BeverageId = 4, ConsumedAt = start, ConsumedAmountMl = 500, TotalCaffeineMg = 9999 });
            await db.SaveChangesAsync();
            var summary = await s.GetRequiredService<WeeklySummaryService>().GetAsync(1, start);
            Assert.Equal(160, summary.Current.TotalMg);
            Assert.Equal(1, summary.Sleep.Count);
            Assert.Equal(7.5, summary.Sleep.AverageHours);
            Assert.False(summary.CanCompare);
            return 0;
        });
    }

    [Fact]
    public async Task VerificationTokenIsHashedEncryptedExpiringAndSingleUse()
    {
        var id = await User(confirmed: false);
        await Work(async (s, db) =>
        {
            var c = s.GetRequiredService<EmailConfirmationService>();
            Assert.Equal("VerificationQueued", await c.RequestAsync(id, "de"));
            Assert.Equal("VerificationThrottled", await c.RequestAsync(id, "de"));
            var job = await db.MailJobs.AsNoTracking().SingleAsync();
            var message = await s.GetRequiredService<MailContentRenderer>().RenderAsync(job);
            Assert.NotNull(message);
            // Verification emails use a dedicated CTA, exclude the statistics disclaimer,
            // and start with a light palette for clients that recolor dark-mode emails.
            Assert.Contains("class=\"koffi-button\"", message.Html);
            Assert.Contains(">" + WebUtility.HtmlEncode(message.Subject) + "</a>", message.Html);
            Assert.Contains("name=\"color-scheme\" content=\"light dark\"", message.Html);
            Assert.Contains("background-color:#f1f5f9", message.Html);
            Assert.Equal(2, Regex.Matches(message.Html, "class=\"koffi-copy\"").Count);
            var url = Regex.Match(message.PlainText, @"https://[^\s]+ConfirmEmail\?[^\s]+").Value;
            Assert.NotEmpty(url);
            var token = QueryHelpers.ParseQuery(new Uri(url).Query)["token"].ToString();
            Assert.Equal(43, token.Length);
            var stored = await db.EmailConfirmationTokens.SingleAsync();
            Assert.Equal(64, stored.TokenHash.Length);
            Assert.DoesNotContain(token, job.ProtectedPayload);
            Assert.DoesNotContain(token, stored.TokenHash);
            Assert.True(await c.ValidAsync(token));
            Assert.False(await c.ConfirmAsync("invalid"));
            app.Clock.At = app.Clock.At.AddHours(25);
            Assert.False(await c.ConfirmAsync(token));
            app.Clock.At = app.Clock.At.AddHours(-25);
            Assert.True(await c.ConfirmAsync(token));
            Assert.False(await c.ConfirmAsync(token));
            Assert.True((await db.Users.SingleAsync()).EmailConfirmed);
            return 0;
        });
    }

    [Fact]
    public async Task ConfirmationResendInvalidatesPriorToken()
    {
        var id = await User(confirmed: false);
        await Work(async (s, db) =>
        {
            var c = s.GetRequiredService<EmailConfirmationService>();
            await c.RequestAsync(id, "en");
            var old = await db.EmailConfirmationTokens.AsNoTracking().SingleAsync();
            app.Clock.At = app.Clock.At.AddMinutes(3);
            await c.RequestAsync(id, "en");
            Assert.NotNull((await db.EmailConfirmationTokens.AsNoTracking().SingleAsync(t => t.Id == old.Id)).UsedAtUtc);
            Assert.Equal(2, await db.EmailConfirmationTokens.CountAsync());
            Assert.Equal(1, await db.MailJobs.CountAsync(j => j.State == "Cancelled"));
            return 0;
        });
    }

    [Fact]
    public async Task EmailLanguageSelectorIsSavedIndependentlyOfInterfaceLanguage()
    {
        // Register through HTTP so the test client has a genuine authentication cookie.
        var registered = await Post(client, "/Account/Register", new()
        {
            { "Username", "mail-language" },
            { "Email", "mail-language@example.test" },
            { "Password", "Password-12345" }
        });
        Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);

        await Work(async (_, db) =>
        {
            var user = await db.Users.SingleAsync();
            user.EmailConfirmed = true;
            await db.SaveChangesAsync();
            return 0;
        });

        // A user without saved settings inherits the site language on first visit.
        var first = await client.GetStringAsync("/Account/EmailSettings?culture=de");
        Assert.Contains("name=\"Culture\"", first);
        Assert.Matches("(?s)<option(?=[^>]*value=\"de\")(?=[^>]*selected)[^>]*>", first);

        // Save Hungarian emails while browsing in German.
        var saved = await Post(client, "/Account/EmailSettings?culture=de", new()
        {
            { "Announcements", "true" },
            { "Weekly", "true" },
            { "Culture", "hu" }
        }, "/Account/EmailSettings?culture=de");
        Assert.Equal(HttpStatusCode.Redirect, saved.StatusCode);
        await Work(async (_, db) =>
        {
            var pref = await db.MailingPreferences.AsNoTracking().SingleAsync();
            Assert.Equal("hu", pref.Culture);
            Assert.True(pref.Weekly);
            Assert.True(pref.Announcements);
            return 0;
        });

        // Changing the site language must not silently reset the email preference.
        var persisted = await client.GetStringAsync("/Account/EmailSettings?culture=en");
        Assert.Matches("(?s)<option(?=[^>]*value=\"hu\")(?=[^>]*selected)[^>]*>", persisted);

        // Tampered values are rejected without changing opt-ins or the saved language.
        var invalid = await Post(client, "/Account/EmailSettings?culture=en", new()
        {
            { "Announcements", "true" },
            { "Weekly", "true" },
            { "Culture", "fr" }
        }, "/Account/EmailSettings?culture=en");
        Assert.Equal(HttpStatusCode.OK, invalid.StatusCode);
        Assert.Contains("Choose a supported email language", await invalid.Content.ReadAsStringAsync());
        await Work(async (_, db) =>
        {
            var pref = await db.MailingPreferences.AsNoTracking().SingleAsync();
            Assert.Equal("hu", pref.Culture);
            Assert.True(pref.Weekly);
            Assert.True(pref.Announcements);
            return 0;
        });

        // Changing only the email language does not unsubscribe either category.
        var switched = await Post(client, "/Account/EmailSettings?culture=hu", new()
        {
            { "Announcements", "true" },
            { "Weekly", "true" },
            { "Culture", "de" }
        }, "/Account/EmailSettings?culture=hu");
        Assert.Equal(HttpStatusCode.Redirect, switched.StatusCode);
        await Work(async (_, db) =>
        {
            var pref = await db.MailingPreferences.AsNoTracking().SingleAsync();
            Assert.Equal("de", pref.Culture);
            Assert.True(pref.Weekly);
            Assert.True(pref.Announcements);
            Assert.Equal(2, await db.MailingConsents.CountAsync());
            return 0;
        });
    }

    [Fact]
    public async Task NewsletterConsentIsIndependentVerifiedAndUnsubscribeRejectsTampering()
    {
        var id = await User();
        var other = await User("other@example.test", false);
        await Work(async (s, db) =>
        {
            var p = s.GetRequiredService<MailingPreferenceService>();
            Assert.False(await p.SaveAsync(other, true, true, "hu"));
            Assert.True(await p.SaveAsync(id, true, true, "de"));
            var pref = await db.MailingPreferences.AsNoTracking().SingleAsync();
            var token = p.Token(pref, "weekly");
            Assert.False(await p.UnsubscribeAsync(token + "x"));
            Assert.True(await p.UnsubscribeAsync(token));
            db.ChangeTracker.Clear();
            pref = await db.MailingPreferences.AsNoTracking().SingleAsync();
            Assert.False(pref.Weekly);
            Assert.True(pref.Announcements);
            Assert.True(await p.SaveAsync(id, true, true, "de"));
            Assert.False(await p.UnsubscribeAsync(token));
            Assert.Equal(4, await db.MailingConsents.CountAsync());
            return 0;
        });
    }

    [Fact]
    public async Task PublicMailSettingsRequireAuthenticationAndTokenGetDoesNotMutate()
    {
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/Account/EmailSettings")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Account/ConfirmEmail", new FormUrlEncodedContent(new Dictionary<string, string> { { "token", "bad" } }))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/ConfirmEmail?token=bad")).StatusCode);
        Assert.Equal(0, await Work((_, db) => db.EmailConfirmationTokens.CountAsync()));
    }

    [Fact]
    public async Task WeeklyQueueIsDurableDeduplicatedAndCancelledAfterOptOut()
    {
        var id = await User();
        app.Clock.At = new DateTimeOffset(2026, 9, 1, 8, 0, 0, TimeSpan.Zero);
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailingPreferenceService>().SaveAsync(id, false, true, "en");
            return 0;
        });
        app.Clock.At = new DateTimeOffset(2026, 9, 14, 8, 0, 0, TimeSpan.Zero);
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailOutboxService>().QueueWeeklyAsync();
            await s.GetRequiredService<MailOutboxService>().QueueWeeklyAsync();
            Assert.Equal(1, await db.MailJobs.CountAsync());
            return 0;
        });
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailingPreferenceService>().SaveAsync(id, false, false, "en");
            await s.GetRequiredService<MailOutboxService>().RunOneAsync();
            Assert.Equal("Cancelled", await db.MailJobs.Select(j => j.State).SingleAsync());
            Assert.Empty(app.Email.Outbound);
            return 0;
        });
    }

    private async Task<int> QueueSummary()
    {
        var id = await User();
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailingPreferenceService>().SaveAsync(id, false, true, "en");
            await s.GetRequiredService<MailOutboxService>().EnqueueAsync(id, "weekly", "weekly:2026-09-07", "en", new DateTime(2026, 9, 7), null);
            return 0;
        });
        return id;
    }

    [Fact]
    public async Task WeeklyJobSendsOnlyOnceAcrossServiceScopes()
    {
        await QueueSummary();
        await Work(async (s, db) =>
        {
            Assert.True(await s.GetRequiredService<MailOutboxService>().RunOneAsync());
            return 0;
        });
        await Work(async (s, db) =>
        {
            Assert.False(await s.GetRequiredService<MailOutboxService>().RunOneAsync());
            Assert.Equal("Sent", await db.MailJobs.Select(j => j.State).SingleAsync());
            return 0;
        });
        Assert.Single(app.Email.Outbound);
        Assert.Contains("No sleep entries", app.Email.Outbound.Single().PlainText);
    }

    [Theory]
    [InlineData("retry", "Pending")]
    [InlineData("failed", "Failed")]
    [InlineData("unknown", "Uncertain")]
    public async Task DeliveryFailuresDoNotBecomeSilentSuccess(string failure, string state)
    {
        await QueueSummary();
        app.Email.Failure = failure;
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailOutboxService>().RunOneAsync();
            Assert.Equal(state, await db.MailJobs.Select(j => j.State).SingleAsync());
            return 0;
        });
        Assert.Empty(app.Email.Outbound);
        if (failure == "retry")
        {
            app.Email.Failure = null;
            app.Clock.At = app.Clock.At.AddMinutes(2);
            await Work(async (s, db) =>
            {
                await s.GetRequiredService<MailOutboxService>().RunOneAsync();
                Assert.Equal("Sent", await db.MailJobs.Select(j => j.State).SingleAsync());
                return 0;
            });
            Assert.Single(app.Email.Outbound);
        }
    }

    [Fact]
    public async Task LostSmtpAcknowledgementIsNeverBlindlyRetriedAfterRestart()
    {
        await QueueSummary();
        await Work(async (s, db) =>
        {
            await db.MailJobs.ExecuteUpdateAsync(j => j.SetProperty(x => x.State, "Sending").SetProperty(x => x.LeaseUntilUtc, app.Clock.At.UtcDateTime.AddMinutes(-1)));
            await s.GetRequiredService<MailOutboxService>().RecoverLeasesAsync();
            Assert.Equal("Uncertain", await db.MailJobs.Select(j => j.State).SingleAsync());
            Assert.False(await s.GetRequiredService<MailOutboxService>().RunOneAsync());
            return 0;
        });
        Assert.Empty(app.Email.Outbound);
    }

    [Fact]
    public async Task CampaignHtmlEscapesContentAndHasPlainTextAndScopedUnsubscribe()
    {
        var id = await User();
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailingPreferenceService>().SaveAsync(id, true, false, "en");
            var c = new MailCampaign
            {
                Key = "test",
                Started = true,
                ContentJson = System.Text.Json.JsonSerializer.Serialize(new CampaignContent(new("HU", "HU"), new("<title>", "<script>alert(1)</script>"), new("DE", "DE")))
            };
            db.MailCampaigns.Add(c);
            await db.SaveChangesAsync();
            var message = await s.GetRequiredService<MailContentRenderer>().RenderAsync(new MailJob { UserId = id, Kind = "announcement", CampaignId = c.Id, Culture = "en" });
            Assert.NotNull(message);
            Assert.DoesNotContain("<script>", message.Html);
            Assert.Contains("&lt;script&gt;", message.Html);
            Assert.Contains("<script>", message.PlainText);
            Assert.NotNull(message.UnsubscribeUrl);
            return 0;
        });
    }

    [Fact]
    public async Task DuplicateWorkersClaimAtMostOneDelivery()
    {
        await QueueSummary();
        await Task.WhenAll(Work(async (s, db) =>
        {
            await s.GetRequiredService<MailOutboxService>().RunOneAsync();
            return 0;
        }), Work(async (s, db) =>
        {
            await s.GetRequiredService<MailOutboxService>().RunOneAsync();
            return 0;
        }));
        Assert.Single(app.Email.Outbound);
    }

    [Fact]
    public async Task DstSleepPersistsRealDurationAndRejectsSkippedTimeWithoutChangingOldRows()
    {
        app.Clock.At = new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero);
        await Work(async (s, db) =>
        {
            var service = s.GetRequiredService<SleepService>();
            var form = new SleepForm
            {
                ActualBedtime = new DateTime(2026, 10, 25, 2, 15, 0),
                WakeTime = new DateTime(2026, 10, 25, 3, 15, 0),
                BedtimeOffsetMinutes = 120,
                WakeOffsetMinutes = 60,
                SleepRating = 4
            };
            Assert.Null(await service.SaveAsync("owner", form));
            var row = await db.SleepLogs.AsNoTracking().SingleAsync();
            Assert.Equal(2, row.DurationHours);
            Assert.Null(row.Awakenings);
            Assert.Null(row.FallingAsleep);
            Assert.Equal(new DateTime(2026, 10, 25), row.SleepDate);
            form.Id = row.Id;
            form.ActualBedtime = new DateTime(2026, 3, 29, 2, 15, 0);
            Assert.Equal("SleepClockInvalid", await service.SaveAsync("owner", form));
            Assert.Equal(2, (await db.SleepLogs.AsNoTracking().SingleAsync()).DurationHours);
            return 0;
        });
    }

    [Theory]
    [InlineData("hu", "7 óra 30 perc")]
    [InlineData("en", "7 h 30 min")]
    [InlineData("de", "7 Std. 30 Min.")]
    public async Task SleepDurationIsLocalizedHoursAndMinutes(string culture, string expected)
    {
        await Work((s, _) =>
        {
            var old = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
                Assert.Equal(expected, s.GetRequiredService<DurationDisplay>().Hours(7.5));
            }
            finally
            {
                CultureInfo.CurrentUICulture = old;
            }

            return Task.FromResult(0);
        });
    }

    [Fact]
    public async Task RegistrationQueuesVerificationButDoesNotForceSubscriptions()
    {
        var response = await Post(client, "/Account/Register", new() { { "Username", "new" }, { "Email", "new@example.test" }, { "Password", "Password-12345" } });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await Work(async (_, db) =>
        {
            var u = await db.Users.SingleAsync();
            Assert.True(u.EmailVerificationRequired);
            Assert.False(u.EmailConfirmed);
            Assert.Equal("verify", await db.MailJobs.Select(j => j.Kind).SingleAsync());
            Assert.Empty(await db.MailingPreferences.ToListAsync());
            return 0;
        });
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Account/EmailSettings")).StatusCode);
    }

    [Fact]
    public async Task CampaignMustBeTestedAndSendingIsIdempotent()
    {
        var id = await User();
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailingPreferenceService>().SaveAsync(id, true, false, "en");
            db.MailCampaigns.Add(new() { Key = "release-test", ContentJson = System.Text.Json.JsonSerializer.Serialize(new CampaignContent(new("HU", "HU"), new("EN", "EN"), new("DE", "DE"))) });
            await db.SaveChangesAsync();
            return 0;
        });
        var command = new[]
        {
            "--mail-campaign-send",
            "--key",
            "release-test",
            "--confirm",
            "release-test"
        };
        Assert.Equal(2, await MailAdminCommand.RunAsync(command, app.Services));
        Assert.Equal(0, await MailAdminCommand.RunAsync(new[] { "--mail-test", "--key", "release-test", "--user", id.ToString(), "--culture", "en" }, app.Services));
        await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailOutboxService>().RunOneAsync();
            Assert.NotNull((await db.MailCampaigns.AsNoTracking().SingleAsync()).TestSentAtUtc);
            return 0;
        });
        Assert.Equal(0, await MailAdminCommand.RunAsync(command, app.Services));
        Assert.Equal(0, await MailAdminCommand.RunAsync(command, app.Services));
        Assert.Equal(1, await Work((_, db) => db.MailJobs.CountAsync(j => j.Kind == "announcement")));
    }

    [Fact]
    public async Task UnknownDeliveryCannotBeRetriedByOperatorCommand()
    {
        await QueueSummary();
        app.Email.Failure = "unknown";
        var id = await Work(async (s, db) =>
        {
            await s.GetRequiredService<MailOutboxService>().RunOneAsync();
            return await db.MailJobs.Select(j => j.Id).SingleAsync();
        });
        Assert.Equal(2, await MailAdminCommand.RunAsync(new[] { "--mail-retry", "--job", id.ToString(), "--confirm", id.ToString() }, app.Services));
        Assert.Equal(0, await MailAdminCommand.RunAsync(new[] { "--mail-resolve", "--job", id.ToString(), "--state", "cancelled", "--confirm", id.ToString() }, app.Services));
        Assert.Equal("Cancelled", await Work((_, db) => db.MailJobs.Select(j => j.State).SingleAsync()));
    }

    [Fact]
    public async Task CleanupRemovesExpiredVerificationDataButKeepsDeliveryDeduplication()
    {
        var id = await User();
        await Work(async (services, db) =>
        {
            var old = app.Clock.At.UtcDateTime.AddDays(-40);
            db.EmailConfirmationTokens.Add(new() { UserId=id,Email="person@example.test",TokenHash="expired-hash",CreatedAtUtc=old,ExpiresAtUtc=old.AddDays(1) });
            db.MailJobs.AddRange(new MailJob { UserId=id,Kind="verify",DeduplicationKey="verify-old",State="Sent",CreatedAtUtc=old,NextAttemptUtc=old },
                new MailJob { UserId=id,Kind="weekly",DeduplicationKey="weekly-old",State="Sent",CreatedAtUtc=old,NextAttemptUtc=old });
            await db.SaveChangesAsync();
            await services.GetRequiredService<MailOutboxService>().CleanupAsync();
            Assert.Empty(await db.EmailConfirmationTokens.AsNoTracking().ToListAsync());
            Assert.Equal("weekly", await db.MailJobs.AsNoTracking().Select(j=>j.Kind).SingleAsync());
            return 0;
        });
    }

    public void Dispose()
    {
        client.Dispose();
        app.Dispose();
    }
}
