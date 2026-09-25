using System.Net;
using Caffeine.Models;
using Caffeine.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Caffeine.Tests;

public sealed partial class TrackerTests
{
    private static Dictionary<string, string> SleepFormData() => new()
    {
        ["ActualBedtime"] = "2026-09-16T23:30",
        ["WakeTime"] = "2026-09-17T07:30",
        ["PlannedBedtime"] = "2026-09-16T23:00",
        ["SleepRating"] = "4",
        ["FallingAsleep"] = "2",
        ["Awakenings"] = "1",
        ["Notes"] = "A private sleep note"
    };

    [Theory]
    [InlineData("/Sleep")]
    [InlineData("/Sleep/Edit")]
    [InlineData("/Planning")]
    [InlineData("/Planning/Settings")]
    [InlineData("/Stats")]
    [InlineData("/Notifications")]
    [InlineData("/Account/ForgotPassword")]
    public async Task NewPagesRenderInBothLanguagesWithoutSavingData(string page)
    {
        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync(page + "?culture=hu")).StatusCode);

        Assert.Equal(
            HttpStatusCode.OK,
            (await client.GetAsync(page + "?culture=en")).StatusCode);

        Assert.Equal(
            0,
            await Db(db => db.CaffeineLogs.CountAsync()));

        Assert.Equal(
            0,
            await Db(db => db.SleepLogs.CountAsync()));

        Assert.Equal(
            0,
            await Db(db => db.TrackerPreferences.CountAsync()));
    }

    [Theory]
    [InlineData("/Sleep/Edit")]
    [InlineData("/Sleep/Delete")]
    [InlineData("/Planning")]
    [InlineData("/Planning/Settings")]
    [InlineData("/Notifications/Subscribe")]
    [InlineData("/Notifications/Unsubscribe")]
    [InlineData("/Notifications/UnsubscribeAll")]
    [InlineData("/Account/ForgotPassword")]
    [InlineData("/Account/ResetPassword")]
    public async Task NewPostActionsRejectMissingCsrf(string path) =>
        Assert.Equal(
            HttpStatusCode.BadRequest,
            (
                await client.PostAsync(
                    path,
                    new FormUrlEncodedContent(
                        new Dictionary<string, string>()))
            ).StatusCode);

    [Fact]
    public async Task SleepCrudLinksSnapshotsAndIsolatesUsers()
    {
        var drink = Drink();

        drink["ConsumedAt"] =
            "2026-09-16T18:00";

        await Post(
            client,
            "/Tracker/LogDrink",
            drink,
            "/Tracker/LogDrink");

        Assert.Equal(
            HttpStatusCode.Redirect,
            (
                await Post(
                    client,
                    "/Sleep/Edit",
                    SleepFormData(),
                    "/Sleep/Edit")
            ).StatusCode);

        var row =
            await Db(
                db => db.SleepLogs.SingleAsync());

        Assert.Equal(
            new DateTime(2026, 9, 17),
            row.SleepDate);

        Assert.Equal(
            8,
            row.DurationHours);

        Assert.Equal(
            160,
            row.PreviousDayCaffeineMg);

        Assert.InRange(
            row.EstimatedCaffeineAtBedtime,
            80,
            90);

        Assert.Equal(
            new DateTime(
                2026,
                9,
                16,
                18,
                0,
                0),
            row.LastCaffeineAt);

        using var outsider =
            app.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

        Assert.DoesNotContain(
            "A private sleep note",
            await outsider.GetStringAsync("/Sleep"));

        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await outsider.GetAsync(
                    "/Sleep/Edit?id=" + row.Id)
            ).StatusCode);

        var edit =
            SleepFormData();

        edit["Id"] =
            row.Id.ToString();

        edit["SleepRating"] =
            "1";

        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await Post(
                    outsider,
                    "/Sleep/Edit",
                    edit,
                    "/Sleep/Edit")
            ).StatusCode);

        await Post(
            outsider,
            "/Sleep/Delete",
            new()
            {
                ["id"] = row.Id.ToString()
            });

        Assert.Equal(
            4,
            await Db(
                db =>
                    db.SleepLogs
                        .Select(s => s.SleepRating)
                        .SingleAsync()));

        await Post(
            client,
            "/Sleep/Edit",
            edit,
            "/Sleep/Edit");

        Assert.Equal(
            1,
            await Db(
                db =>
                    db.SleepLogs
                        .Select(s => s.SleepRating)
                        .SingleAsync()));

        await Post(
            client,
            "/Sleep/Delete",
            new()
            {
                ["id"] = row.Id.ToString()
            });

        Assert.Equal(
            0,
            await Db(
                db => db.SleepLogs.CountAsync()));
    }

    [Fact]
    public async Task SleepRejectsInvalidAndDuplicateRequestsAndEscapesNotes()
    {
        var form =
            SleepFormData();

        form["WakeTime"] =
            "2026-09-19T07:00";

        await Post(
            client,
            "/Sleep/Edit",
            form,
            "/Sleep/Edit");

        form =
            SleepFormData();

        form["ActualBedtime"] =
            "2026-09-17T08:00";

        await Post(
            client,
            "/Sleep/Edit",
            form,
            "/Sleep/Edit");

        form =
            SleepFormData();

        form["SleepRating"] =
            "6";

        await Post(
            client,
            "/Sleep/Edit",
            form,
            "/Sleep/Edit");

        Assert.Equal(
            0,
            await Db(
                db => db.SleepLogs.CountAsync()));

        form =
            SleepFormData();

        form["Notes"] =
            "<script>alert(1)</script>";

        await Post(
            client,
            "/Sleep/Edit",
            form,
            "/Sleep/Edit");

        await Post(
            client,
            "/Sleep/Edit",
            form,
            "/Sleep/Edit");

        Assert.Equal(
            1,
            await Db(
                db => db.SleepLogs.CountAsync()));

        var html =
            await client.GetStringAsync("/Sleep");

        Assert.DoesNotContain(
            "<script>alert(1)</script>",
            html);

        Assert.Contains(
            "&lt;script&gt;",
            html);
    }

    [Fact]
    public async Task CalculatorNeverWritesAndPrefillStillRequiresExplicitLogging()
    {
        var result =
            await Post(
                client,
                "/Planning",
                new()
                {
                    ["BeverageId"] = "3",
                    ["AmountMl"] = "500",
                    ["DoseMg"] = "80",
                    ["Bedtime"] = "2026-09-17T23:00",
                    ["IntakeAt"] = "2026-09-17T14:00",
                    ["TargetMg"] = "25"
                },
                "/Planning");

        Assert.Equal(
            HttpStatusCode.OK,
            result.StatusCode);

        Assert.Contains(
            "planning-chart",
            await result.Content.ReadAsStringAsync());

        Assert.Equal(
            0,
            await Db(
                db => db.CaffeineLogs.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.CaffeineFreeDays.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.TrackerPreferences.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.FavoriteDrinks.CountAsync()));

        await client.GetAsync(
            "/Tracker/LogDrink?beverageId=3&amountMl=500");

        Assert.Equal(
            0,
            await Db(
                db => db.CaffeineLogs.CountAsync()));

        await Post(
            client,
            "/Tracker/LogDrink",
            Drink(),
            "/Tracker/LogDrink");

        var owner =
            await Db(
                db =>
                    db.CaffeineLogs
                        .Select(l => l.UserId)
                        .SingleAsync());

        Assert.Equal(
            new StreakSummary(
                1,
                1,
                true),
            await Summary(owner));
    }

    [Fact]
    public async Task CalculatorCannotReadAnotherUsersCustomDrink()
    {
        await Post(
            client,
            "/Tracker/LogDrink",
            Drink(true),
            "/Tracker/LogDrink");

        var id =
            await Db(
                db =>
                    db.Beverages
                        .Where(
                            b => b.Category == "Custom")
                        .Select(b => b.Id)
                        .SingleAsync());

        using var outsider =
            app.CreateClient(
                new WebApplicationFactoryClientOptions
                {
                    AllowAutoRedirect = false
                });

        Assert.DoesNotContain(
            "Private brew",
            await outsider.GetStringAsync("/Planning"));

        Assert.Equal(
            HttpStatusCode.NotFound,
            (
                await Post(
                    outsider,
                    "/Planning",
                    new()
                    {
                        ["BeverageId"] = id.ToString(),
                        ["AmountMl"] = "250",
                        ["DoseMg"] = "80",
                        ["TargetMg"] = "25",
                        ["Bedtime"] = "2026-09-17T23:00",
                        ["IntakeAt"] = "2026-09-17T14:00"
                    },
                    "/Planning")
            ).StatusCode);
    }

    [Fact]
    public async Task LifetimeStatsUseCalendarDenominatorsAndScopeEveryQuery()
    {
        await Post(
            client,
            "/Tracker/LogDrink",
            Drink(),
            "/Tracker/LogDrink");

        var owner =
            await Db(
                db =>
                    db.CaffeineLogs
                        .Select(l => l.UserId)
                        .SingleAsync());

        await Db(
            async db =>
            {
                db.CaffeineLogs.AddRange(
                    new CaffeineLog
                    {
                        UserId = owner,
                        BeverageId = 3,
                        ConsumedAt =
                            new(
                                2026,
                                9,
                                15,
                                8,
                                0,
                                0),
                        TotalCaffeineMg = 80,
                        ConsumedAmountMl = 250
                    },
                    new CaffeineLog
                    {
                        UserId = "other",
                        BeverageId = 3,
                        ConsumedAt =
                            new(
                                2026,
                                9,
                                17,
                                9,
                                0,
                                0),
                        TotalCaffeineMg = 9999
                    });

                db.CaffeineFreeDays.Add(
                    new()
                    {
                        UserId = owner,
                        Day =
                            new(
                                2026,
                                9,
                                16)
                    });

                await db.SaveChangesAsync();

                var service =
                    new LifetimeStatsService(
                        db,
                        new StreakService(db),
                        new SleepInsightService());

                var stats =
                    await service.GetAsync(
                        owner,
                        new DateTime(
                            2026,
                            9,
                            17,
                            14,
                            0,
                            0));

                Assert.Equal(
                    2,
                    stats.DrinkCount);

                Assert.Equal(
                    240,
                    stats.TotalMg);

                Assert.Equal(
                    80,
                    stats.DailyAverage);

                Assert.Equal(
                    240d / 7,
                    stats.Average7);

                Assert.Equal(
                    8,
                    stats.Average30);

                Assert.Equal(
                    1,
                    stats.FreeDayCount);

                Assert.Equal(
                    3,
                    stats.Streak.Current);

                Assert.Equal(
                    365,
                    stats.Heatmap.Count);

                Assert.Equal(
                    TimeSpan.FromHours(9),
                    stats.FirstCaffeineTime);

                Assert.Equal(
                    TimeSpan.FromHours(9),
                    stats.LastCaffeineTime);

                Assert.Equal(
                    160,
                    stats.HighestDay!.Mg);

                Assert.Single(
                    stats.Categories);

                return 0;
            });

        Assert.DoesNotContain(
            "9999",
            await client.GetStringAsync("/Stats"));
    }

    [Fact]
    public async Task FeatureDataFollowsGuestRegistrationAndAccountDeletion()
    {
        await Post(
            client,
            "/Sleep/Edit",
            SleepFormData(),
            "/Sleep/Edit");

        await Post(
            client,
            "/Planning/Settings",
            new()
            {
                ["PlannedBedtime"] = "22:30",
                ["TargetMg"] = "20"
            },
            "/Planning/Settings");

        await Post(
            client,
            "/Notifications",
            new()
            {
                ["StreakReminder"] = "true",
                ["StreakTime"] = "20:00",
                ["MorningTime"] = "08:00",
                ["CutoffDoseMg"] = "80"
            },
            "/Notifications");

        await Post(
            client,
            "/Account/Register",
            new()
            {
                ["Username"] = "featureuser",
                ["Email"] = "feature@example.test",
                ["Password"] = "Test-pass123"
            });

        var owner =
            (
                await Db(
                    db =>
                        db.Users
                            .Select(u => u.Id)
                            .SingleAsync())
            ).ToString();

        Assert.Equal(
            owner,
            await Db(
                db =>
                    db.SleepLogs
                        .Select(s => s.UserId)
                        .SingleAsync()));

        Assert.Equal(
            owner,
            await Db(
                db =>
                    db.TrackerPreferences
                        .Select(s => s.UserId)
                        .SingleAsync()));

        Assert.Equal(
            owner,
            await Db(
                db =>
                    db.NotificationPreferences
                        .Select(s => s.UserId)
                        .SingleAsync()));

        await Post(
            client,
            "/Account/DeleteAccount",
            new());

        Assert.Equal(
            0,
            await Db(
                db => db.SleepLogs.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.TrackerPreferences.CountAsync()));

        Assert.Equal(
            0,
            await Db(
                db => db.NotificationPreferences.CountAsync()));
    }
}