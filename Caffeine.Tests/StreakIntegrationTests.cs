using System.Net;
using Caffeine.Models;
using Caffeine.Repositories;
using Caffeine.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Caffeine.Tests;

public sealed partial class TrackerTests
{
    private static Dictionary<string, string> Today() =>
        new()
        {
            ["date"] = "2026-09-17"
        };

    private Task<StreakSummary> Summary(
        string userId) =>
        Db(db =>
            new StreakService(db)
                .GetAsync(
                    userId,
                    new DateTime(
                        2026,
                        9,
                        17,
                        14,
                        0,
                        0)));


    [Theory]
    [InlineData("/Tracker/MarkCaffeineFree")]
    [InlineData("/Tracker/UnmarkCaffeineFree")]
    public async Task FreeDayActionsRequireAntiforgeryAndPost(
        string path)
    {
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await client.PostAsync(
                    path,
                    new FormUrlEncodedContent(
                        Today()))
            ).StatusCode);

        Assert.Equal(
            HttpStatusCode.MethodNotAllowed,
            (
                await client.GetAsync(path)
            ).StatusCode);

        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));
    }


    [Fact]
    public async Task FreeDayIsIdempotentPrivateLocalizedAndCanBeUndone()
    {
        Assert.Equal(
            HttpStatusCode.Redirect,
            (
                await Post(
                    client,
                    "/Tracker/MarkCaffeineFree",
                    Today())
            ).StatusCode);


        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        var marker =
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .SingleAsync());


        Assert.StartsWith(
            "Guest_",
            marker.UserId);


        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(
                marker.UserId));


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineLogs
                        .CountAsync()));


        var en =
            WebUtility.HtmlDecode(
                await client.GetStringAsync(
                    "/?culture=en"));


        Assert.Contains(
            "Today is complete.",
            en);

        Assert.Contains(
            "Undo caffeine-free day",
            en);


        Assert.Contains(
            "Koffeinmentes",
            WebUtility.HtmlDecode(
                await client.GetStringAsync(
                    "/?culture=hu")));


        Assert.Contains(
            "Caffeine-free day",
            await client.GetStringAsync(
                "/Tracker/Week?culture=en"));


        using var outsider =
            app.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });


        await Post(
            outsider,
            "/Tracker/UnmarkCaffeineFree",
            Today());


        Assert.Equal(
            1,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));


        Assert.DoesNotContain(
            "Undo caffeine-free day",
            await outsider.GetStringAsync(
                "/?culture=en"));


        await Post(
            client,
            "/Tracker/UnmarkCaffeineFree",
            Today());

        await Post(
            client,
            "/Tracker/UnmarkCaffeineFree",
            Today());


        Assert.Equal(
            new StreakSummary(
                0,
                0,
                false),
            await Summary(
                marker.UserId));
    }


    [Fact]
    public async Task StaleOrForgedDatesCannotCompleteAnotherDay()
    {
        foreach (
            var date in new[]
            {
                "2026-09-16",
                "2026-09-18",
                "invalid",
                ""
            })
        {
            await Post(
                client,
                "/Tracker/MarkCaffeineFree",
                new()
                {
                    ["date"] = date
                });
        }


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));


        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        await Post(
            client,
            "/Tracker/UnmarkCaffeineFree",
            new()
            {
                ["date"] =
                    "2026-09-16"
            });


        Assert.Equal(
            1,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));
    }


    [Fact]
    public async Task CaffeineClearsMarkerAndCannotBeMarkedFreeUntilLastLogIsDeleted()
    {
        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        var user =
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .Select(
                            d =>
                                d.UserId)
                        .SingleAsync());


        await Post(
            client,
            "/Tracker/LogDrink",
            Drink(),
            "/Tracker/LogDrink");


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));


        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));


        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(user));


        var id =
            await Db(
                db =>
                    db.CaffeineLogs
                        .Select(
                            l =>
                                l.Id)
                        .SingleAsync());


        await Post(
            client,
            "/Tracker/DeleteLog",
            new()
            {
                ["id"] =
                    id.ToString()
            });


        Assert.Equal(
            new StreakSummary(
                0,
                0,
                false),
            await Summary(user));


        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        var favorite =
            await Db(
                db =>
                    db.FavoriteDrinks
                        .Select(
                            f =>
                                f.Id)
                        .SingleAsync());


        await Post(
            client,
            "/Tracker/QuickAdd",
            new()
            {
                ["id"] =
                    favorite.ToString()
            });


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));


        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(user));


        id =
            await Db(
                db =>
                    db.CaffeineLogs
                        .Select(
                            l =>
                                l.Id)
                        .SingleAsync());


        await Post(
            client,
            "/Tracker/UndoQuickAdd",
            new()
            {
                ["id"] =
                    id.ToString()
            });


        Assert.Equal(
            new StreakSummary(
                0,
                0,
                false),
            await Summary(user));
    }


    [Fact]
    public async Task ZeroCaffeineDrinkAndMarkerCountAsOneDayAndBackdatedLogsJoinStreak()
    {
        var drink =
            Drink(true);


        drink["CustomCaffeinePer100Ml"] =
            "0";


        await Post(
            client,
            "/Tracker/LogDrink",
            drink,
            "/Tracker/LogDrink");


        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        var user =
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .Select(
                            d =>
                                d.UserId)
                        .SingleAsync());


        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(user));


        await Post(
            client,
            "/Tracker/UnmarkCaffeineFree",
            Today());


        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(user));


        drink =
            Drink();


        drink["ConsumedAt"] =
            "2026-09-16T10:00";


        await Post(
            client,
            "/Tracker/LogDrink",
            drink,
            "/Tracker/LogDrink");


        Assert.Equal(
            new StreakSummary(
                2,
                2,
                true),
            await Summary(user));
    }


    [Fact]
    public async Task MarkerTransfersAtRegistrationAndAccountDeletionCleansItUp()
    {
        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        Assert.Equal(
            HttpStatusCode.Redirect,
            (
                await Post(
                    client,
                    "/Account/Register",
                    new()
                    {
                        ["Username"] =
                            "streakuser",

                        ["Email"] =
                            "streak@example.test",

                        ["Password"] =
                            "Test-pass123"
                    })
            ).StatusCode);


        var account =
            (
                await Db(
                    db =>
                        db.Users
                            .Select(
                                u =>
                                    u.Id)
                            .SingleAsync())
            ).ToString();


        Assert.Equal(
            account,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .Select(
                            d =>
                                d.UserId)
                        .SingleAsync()));


        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(account));


        await Post(
            client,
            "/Account/DeleteAccount",
            new());


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .CountAsync()));
    }


    [Fact]
    public async Task GuestMergeUnionsDatesAndRemovesConflictsWithEitherUsersCaffeine()
    {
        const string guest =
            "Guest_33333333-3333-4333-8333-333333333333";


        await Db(
            async db =>
            {
                foreach (
                    var user in new[]
                    {
                        guest,
                        "42"
                    })
                {
                    foreach (
                        var day in new[]
                        {
                            14,
                            15,
                            16
                        })
                    {
                        db.CaffeineFreeDays.Add(
                            new()
                            {
                                UserId =
                                    user,

                                Day =
                                    new DateTime(
                                        2026,
                                        9,
                                        day)
                            });
                    }
                }


                db.CaffeineFreeDays.Add(
                    new()
                    {
                        UserId =
                            guest,

                        Day =
                            new DateTime(
                                2026,
                                9,
                                17)
                    });


                db.CaffeineLogs.AddRange(
                    new()
                    {
                        UserId =
                            "42",

                        BeverageId =
                            3,

                        ConsumedAt =
                            new DateTime(
                                2026,
                                9,
                                14,
                                10,
                                0,
                                0),

                        TotalCaffeineMg =
                            123.4,

                        ConsumedAmountMl =
                            500
                    },

                    new()
                    {
                        UserId =
                            guest,

                        BeverageId =
                            3,

                        ConsumedAt =
                            new DateTime(
                                2026,
                                9,
                                15,
                                10,
                                0,
                                0),

                        TotalCaffeineMg =
                            22.2,

                        ConsumedAmountMl =
                            250
                    });


                await db.SaveChangesAsync();


                var repo =
                    new CaffeineLogRepository(
                        db,
                        new StreakService(db));


                await repo.TransferLogsAsync(
                    guest,
                    "42");

                await repo.TransferLogsAsync(
                    guest,
                    "42");


                Assert.Equal(
                    new[]
                    {
                        16,
                        17
                    },
                    await db.CaffeineFreeDays
                        .OrderBy(
                            d =>
                                d.Day)
                        .Select(
                            d =>
                                d.Day.Day)
                        .ToArrayAsync());


                Assert.All(
                    await db.CaffeineFreeDays
                        .ToListAsync(),
                    d =>
                        Assert.Equal(
                            "42",
                            d.UserId));


                Assert.Equal(
                    new[]
                    {
                        123.4,
                        22.2
                    },
                    await db.CaffeineLogs
                        .OrderBy(
                            l =>
                                l.Id)
                        .Select(
                            l =>
                                l.TotalCaffeineMg)
                        .ToArrayAsync());


                return 0;
            });


        Assert.Equal(
            new StreakSummary(
                4,
                4,
                true),
            await Summary("42"));
    }


    [Fact]
    public async Task HistoricalFreeDayAppearsInDailyAndWeeklyViewsWithoutAFakeDrink()
    {
        await Post(
            client,
            "/Tracker/MarkCaffeineFree",
            Today());


        var user =
            await Db(
                db =>
                    db.CaffeineFreeDays
                        .Select(
                            d =>
                                d.UserId)
                        .SingleAsync());


        await Db(
            async db =>
            {
                db.CaffeineFreeDays.Add(
                    new()
                    {
                        UserId =
                            user,

                        Day =
                            new DateTime(
                                2026,
                                9,
                                16)
                    });

                return await db.SaveChangesAsync();
            });


        var html =
            await client.GetStringAsync(
                "/?date=2026-09-16&culture=en");


        Assert.Contains(
            "Caffeine-free day",
            html);


        Assert.DoesNotContain(
            "No caffeine today",
            html);


        Assert.Equal(
            new StreakSummary(
                2,
                2,
                true),
            await Summary(user));


        Assert.Equal(
            0,
            await Db(
                db =>
                    db.CaffeineLogs
                        .CountAsync()));
    }
}