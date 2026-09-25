using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Caffeine.Data;
using Caffeine.Models;
using Caffeine.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace Caffeine.Tests;

public sealed partial class TrackerTests : IDisposable
{
    private readonly TestApp app = new();
    private readonly HttpClient client;

    public TrackerTests()
    {
        client = app.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        using var scope = app.Services.CreateScope();

        DatabaseUpgrade.ApplyAsync(
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>())
            .GetAwaiter()
            .GetResult();
    }

    private async Task<T> Db<T>(
        Func<AppDbContext, Task<T>> work)
    {
        using var scope = app.Services.CreateScope();

        return await work(
            scope.ServiceProvider
                .GetRequiredService<AppDbContext>());
    }

    private static async Task<string> Token(
        HttpClient browser,
        string page = "/")
    {
        var response = await browser.GetAsync(page);

        response.EnsureSuccessStatusCode();

        var html =
            await response.Content.ReadAsStringAsync();

        var match = Regex.Match(
            html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");

        Assert.True(
            match.Success,
            "Antiforgery token must be rendered");

        return WebUtility.HtmlDecode(
            match.Groups[1].Value);
    }

    private static async Task<HttpResponseMessage> Post(
        HttpClient browser,
        string path,
        Dictionary<string, string> values,
        string page = "/")
    {
        values["__RequestVerificationToken"] =
            await Token(browser, page);

        return await browser.PostAsync(
            path,
            new FormUrlEncodedContent(values));
    }

    private static Dictionary<string, string> Drink(
        bool custom = false) => new()
    {
        ["IsCustomDrink"] = custom.ToString(),
        ["SelectedBeverageId"] = "3",
        ["AmountMl"] = "500",
        ["ConsumedAt"] = "2026-09-17T10:00",
        ["SaveAsFavorite"] = "true",
        ["CustomBeverageName"] = "Private brew",
        ["CustomCaffeinePer100Ml"] = "38.4"
    };


    [Fact]
    public async Task LegacyCookieCannotReadDeleteOrTransferAccountLogs()
    {
        await Db(async db =>
        {
            db.Users.Add(
                new AppUser
                {
                    Id = 1,
                    Username = "Existing",
                    Email = "existing@example.test",
                    PasswordHash = "test-only"
                });

            db.CaffeineLogs.Add(
                new CaffeineLog
                {
                    UserId = "1",
                    BeverageId = 3,
                    ConsumedAt =
                        new DateTime(2026, 9, 17, 10, 0, 0),
                    TotalCaffeineMg = 777,
                    ConsumedAmountMl = 500
                });

            return await db.SaveChangesAsync();
        });

        client.DefaultRequestHeaders.Add(
            "Cookie",
            "GuestId=1");

        var html = WebUtility.HtmlDecode(
            await client.GetStringAsync("/"));

        Assert.DoesNotContain("777", html);

        var id = await Db(
            db => db.CaffeineLogs
                .Select(l => l.Id)
                .SingleAsync());

        await Post(
            client,
            "/Tracker/DeleteLog",
            new()
            {
                ["id"] = id.ToString()
            });

        await Post(
            client,
            "/Account/Register",
            new()
            {
                ["Username"] = "tester",
                ["Email"] = "tester@example.test",
                ["Password"] = "Test-pass123"
            });

        Assert.Equal(
            "1",
            await Db(
                db => db.CaffeineLogs
                    .Select(l => l.UserId)
                    .SingleAsync()));
    }


    [Fact]
    public async Task ExistingRandomGuestCredentialKeepsItsDataAndGetsProtected()
    {
        const string guest =
            "Guest_22222222-2222-4222-8222-222222222222";

        await Db(async db =>
        {
            db.CaffeineLogs.Add(
                new CaffeineLog
                {
                    UserId = guest,
                    BeverageId = 3,
                    ConsumedAt =
                        new DateTime(2026, 9, 17, 10, 0, 0),
                    ConsumedAmountMl = 500,
                    TotalCaffeineMg = 123.4
                });

            return await db.SaveChangesAsync();
        });

        client.DefaultRequestHeaders.Add(
            "Cookie",
            "GuestId=" + guest);

        var response =
            await client.GetAsync("/?culture=en");

        Assert.Contains(
            "123.4 mg",
            await response.Content.ReadAsStringAsync());

        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            c => c.StartsWith("CaffeineGuestV2="));

        Assert.Equal(
            guest,
            await Db(
                db => db.CaffeineLogs
                    .Select(l => l.UserId)
                    .SingleAsync()));
    }


    [Theory]
    [InlineData("/Account/Login")]
    [InlineData("/Account/Register")]
    [InlineData("/Tracker/QuickAdd")]
    [InlineData("/Tracker/SetTargetSleepTime")]
    [InlineData("/Tracker/SetLanguage")]
    public async Task AllMutatingFormsRejectMissingAntiforgery(
        string endpoint)
    {
        var result = await client.PostAsync(
            endpoint,
            new FormUrlEncodedContent(
                new Dictionary<string, string>()));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            result.StatusCode);
    }


    [Fact]
    public async Task CustomDrinkFavoriteAndLogsTransferAndRemainPrivate()
    {
        var result = await Post(
            client,
            "/Tracker/LogDrink",
            Drink(true),
            "/Tracker/LogDrink");

        Assert.Equal(
            HttpStatusCode.Redirect,
            result.StatusCode);

        var beverage = await Db(
            db => db.Beverages
                .SingleAsync(b => b.Category == "Custom"));

        Assert.StartsWith(
            "Guest_",
            beverage.OwnerId);

        Assert.Equal(
            192,
            await Db(
                db => db.CaffeineLogs
                    .Select(l => l.TotalCaffeineMg)
                    .SingleAsync()));

        var favorite = await Db(
            db => db.FavoriteDrinks.SingleAsync());

        using var outsider =
            app.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

        Assert.DoesNotContain(
            "Private brew",
            await outsider.GetStringAsync(
                "/Tracker/LogDrink"));

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await Post(
                outsider,
                "/Tracker/QuickAdd",
                new()
                {
                    ["id"] = favorite.Id.ToString()
                }))
            .StatusCode);

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await Post(
                outsider,
                "/Tracker/RemoveFavorite",
                new()
                {
                    ["id"] = favorite.Id.ToString()
                }))
            .StatusCode);

        var forged = Drink();

        forged["SelectedBeverageId"] =
            beverage.Id.ToString();

        Assert.Equal(
            HttpStatusCode.NotFound,
            (await Post(
                outsider,
                "/Tracker/LogDrink",
                forged,
                "/Tracker/LogDrink"))
            .StatusCode);

        Assert.Equal(
            HttpStatusCode.Redirect,
            (await Post(
                client,
                "/Account/Register",
                new()
                {
                    ["Username"] = "tester",
                    ["Email"] = "tester@example.test",
                    ["Password"] = "Test-pass123"
                }))
            .StatusCode);

        var owner = await Db(
            db => db.Users
                .Select(u => u.Id)
                .SingleAsync());

        Assert.Equal(
            owner.ToString(),
            await Db(
                db => db.Beverages
                    .Where(b => b.Id == beverage.Id)
                    .Select(b => b.OwnerId)
                    .SingleAsync()));

        Assert.Equal(
            owner.ToString(),
            await Db(
                db => db.FavoriteDrinks
                    .Select(f => f.UserId)
                    .SingleAsync()));

        Assert.Equal(
            owner.ToString(),
            await Db(
                db => db.CaffeineLogs
                    .Select(l => l.UserId)
                    .SingleAsync()));

        Assert.Equal(
            HttpStatusCode.Redirect,
            (await Post(
                client,
                "/Tracker/QuickAdd",
                new()
                {
                    ["id"] = favorite.Id.ToString()
                }))
            .StatusCode);

        var quickId = await Db(
            db => db.CaffeineLogs
                .MaxAsync(l => l.Id));

        Assert.Equal(
            HttpStatusCode.Redirect,
            (await Post(
                client,
                "/Tracker/UndoQuickAdd",
                new()
                {
                    ["id"] = quickId.ToString()
                }))
            .StatusCode);

        Assert.Equal(
            1,
            await Db(
                db => db.CaffeineLogs.CountAsync()));

        await Post(
            client,
            "/Account/DeleteAccount",
            new());

        Assert.Equal(
            0,
            await Db(
                db => db.CaffeineLogs.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.FavoriteDrinks.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.Beverages.CountAsync(
                    b => b.Category == "Custom")));
    }


    [Fact]
    public async Task HistoryIncludesCarryoverAndWeeksCrossYears()
    {
        await Post(
            client,
            "/Tracker/LogDrink",
            Drink(),
            "/Tracker/LogDrink");

        var owner = await Db(
            db => db.CaffeineLogs
                .Select(l => l.UserId)
                .SingleAsync());

        await Db(async db =>
        {
            db.CaffeineLogs.AddRange(
                new CaffeineLog
                {
                    UserId = owner,
                    BeverageId = 3,
                    ConsumedAt =
                        new DateTime(2025, 12, 31, 23, 0, 0),
                    TotalCaffeineMg = 160,
                    ConsumedAmountMl = 500
                },

                new CaffeineLog
                {
                    UserId = owner,
                    BeverageId = 3,
                    ConsumedAt =
                        new DateTime(2026, 1, 1, 10, 0, 0),
                    TotalCaffeineMg = 80,
                    ConsumedAmountMl = 250
                },

                new CaffeineLog
                {
                    UserId = "someone-else",
                    BeverageId = 3,
                    ConsumedAt =
                        new DateTime(2026, 1, 1, 10, 0, 0),
                    TotalCaffeineMg = 9999,
                    ConsumedAmountMl = 250
                });

            return await db.SaveChangesAsync();
        });

        var day = WebUtility.HtmlDecode(
            await client.GetStringAsync(
                "/Tracker/Index?date=2026-01-01&culture=en"));

        Assert.Contains("80 mg", day);
        Assert.DoesNotContain("9999", day);
        Assert.DoesNotContain("Planned bedtime", day);

        Assert.Matches(
            "\"TimeLabel\":\"00:00\",\"ActiveCaffeine\":15[0-9]",
            day);

        var week = WebUtility.HtmlDecode(
            await client.GetStringAsync(
                "/Tracker/Week?date=2026-01-01&culture=en"));

        Assert.Contains("240 mg", week);
        Assert.Contains("2025.12.29.", week);
        Assert.DoesNotContain("9999", week);

        var today =
            await client.GetStringAsync("/?culture=en");

        Assert.Contains(
            "Favorites",
            today);

        var translated =
            await client.GetStringAsync(
                "/Tracker/LogDrink?culture=en");

        Assert.Contains(
            "Double Espresso",
            translated);

        Assert.DoesNotContain(
            "Dupla Espresso",
            translated);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync(
                "/?date=9999-12-31"))
            .StatusCode);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await client.GetAsync(
                "/Tracker/Week?date=garbage"))
            .StatusCode);
    }


    [Fact]
    public async Task TamperedGuestTokenIsReplacedAndHttpOnly()
    {
        client.DefaultRequestHeaders.Add(
            "Cookie",
            "CaffeineGuestV2=invalid");

        var response =
            await client.GetAsync("/");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        Assert.Contains(
            response.Headers.GetValues("Set-Cookie"),
            c =>
                c.StartsWith("CaffeineGuestV2=") &&
                c.Contains(
                    "httponly",
                    StringComparison.OrdinalIgnoreCase));
    }


    [Fact]
    public async Task InvalidTimeAndInvalidCaffeineAreNotSaved()
    {
        var drink = Drink(true);

        drink["ConsumedAt"] =
            "2099-01-01T10:00";

        Assert.Equal(
            HttpStatusCode.OK,
            (await Post(
                client,
                "/Tracker/LogDrink",
                drink,
                "/Tracker/LogDrink"))
            .StatusCode);

        drink = Drink(true);

        drink["CustomCaffeinePer100Ml"] =
            "NaN";

        Assert.Equal(
            HttpStatusCode.OK,
            (await Post(
                client,
                "/Tracker/LogDrink",
                drink,
                "/Tracker/LogDrink"))
            .StatusCode);

        Assert.Equal(
            0,
            await Db(
                db => db.CaffeineLogs.CountAsync()));

        Assert.Equal(
            HttpStatusCode.BadRequest,
            (await Post(
                client,
                "/Tracker/SetTargetSleepTime",
                new()
                {
                    ["time"] = "24:99"
                }))
            .StatusCode);
    }


    [Fact]
    public void AbsorptionAndHalfLifeRespectBoundaries()
    {
        var strategy =
            new StandardCaffeineDecayStrategy();

        var at =
            new DateTime(2026, 9, 17, 10, 0, 0);

        Assert.Equal(
            0,
            strategy.CalculateActiveCaffeine(
                160,
                at,
                at));

        Assert.Equal(
            80,
            strategy.CalculateActiveCaffeine(
                160,
                at,
                at.AddMinutes(22.5)));

        Assert.Equal(
            160,
            strategy.CalculateActiveCaffeine(
                160,
                at,
                at.AddMinutes(45)));

        Assert.Equal(
            80,
            strategy.CalculateActiveCaffeine(
                160,
                at,
                at.AddMinutes(345)));
    }


    public void Dispose()
    {
        client.Dispose();
        app.Dispose();
    }
}


public sealed class TestApp : WebApplicationFactory<Program>
{
    private readonly string dbPath =
        Path.Combine(
            Path.GetTempPath(),
            $"caffeine-tests-{Guid.NewGuid():N}.db");


    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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
                            $"Data Source={dbPath}"));

                services.RemoveAll<TimeProvider>();

                services.AddSingleton<TimeProvider>(
                    new FixedClock());
            });
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            Microsoft.Data.Sqlite
                .SqliteConnection
                .ClearAllPools();

            File.Delete(dbPath);
        }
    }
}


public sealed class FixedClock : TimeProvider
{
    public override DateTimeOffset GetUtcNow() =>
        new(
            2026,
            9,
            17,
            12,
            0,
            0,
            TimeSpan.Zero);
}