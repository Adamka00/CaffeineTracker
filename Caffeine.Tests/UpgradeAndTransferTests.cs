using Caffeine.Data;
using Caffeine.Repositories;
using Caffeine.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Caffeine.Tests;

public sealed class UpgradeAndTransferTests
{
    [Fact]
    public async Task AlreadyInstalledStreakMigrationKeepsMarkersAndLegacyAccountsOnUpgrade()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        await using var db =
            new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(connection)
                    .Options);

        await db
            .GetService<IMigrator>()
            .MigrateAsync(
                "20260821170905_3.0");

        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE CaffeineLogs ADD COLUMN UserId TEXT NOT NULL DEFAULT '';

            CREATE TABLE Users (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Email TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );

            INSERT INTO Users
            VALUES (
                42,
                'Existing',
                'existing@example.test',
                'keep-hash',
                '2026-08-21 12:00:00'
            );
            """);

        await db
            .GetService<IMigrator>()
            .MigrateAsync(
                "20260923092952_StreakCaffeineFreeDays");

        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO CaffeineFreeDays (UserId, Day)
            VALUES ('42', '2026-09-17 00:00:00');

            INSERT INTO CaffeineLogs (
                Id,
                BeverageId,
                ConsumedAt,
                ConsumedAmountMl,
                TotalCaffeineMg,
                UserId
            )
            VALUES (
                456,
                3,
                '2026-09-16 18:00:00',
                500,
                157.2,
                '42'
            );
            """);

        await DatabaseUpgrade.ApplyAsync(db);
        await DatabaseUpgrade.ApplyAsync(db);

        var user =
            await db.Users.SingleAsync();

        Assert.Equal(
            "keep-hash",
            user.PasswordHash);

        Assert.Equal(
            "",
            user.SecurityStamp);

        var log =
            await db.CaffeineLogs.SingleAsync();

        Assert.Equal(
            456,
            log.Id);

        Assert.Equal(
            157.2,
            log.TotalCaffeineMg);

        Assert.Equal(
            "42",
            (
                await db.CaffeineFreeDays
                    .SingleAsync()
            ).UserId);

        Assert.Equal(
            new StreakSummary(
                2,
                2,
                true),
            await new StreakService(db)
                .GetAsync(
                    "42",
                    new DateTime(
                        2026,
                        9,
                        17,
                        14,
                        0,
                        0)));

        Assert.False(
            db.Database
                .HasPendingModelChanges());

        Assert.Empty(
            await db.SleepLogs
                .ToListAsync());

        Assert.Empty(
            await db.PasswordResetTokens
                .ToListAsync());
    }

    [Fact]
    public async Task GuestMergePreservesConflictingSleepEntriesAndAllNewFeatureData()
    {
        await using var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        await using var db =
            new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(connection)
                    .Options);

        await DatabaseUpgrade.ApplyAsync(db);

        const string guest =
            "Guest_44444444-4444-4444-8444-444444444444";

        foreach (
            var user in new[]
            {
                guest,
                "42"
            })
        {
            db.SleepLogs.Add(
                new()
                {
                    UserId = user,

                    SleepDate =
                        new DateTime(
                            2026,
                            9,
                            17),

                    ActualBedtime =
                        new DateTime(
                            2026,
                            9,
                            16,
                            23,
                            0,
                            0),

                    WakeTime =
                        new DateTime(
                            2026,
                            9,
                            17,
                            7,
                            0,
                            0),

                    SleepRating = 3,
                    Notes = user
                });
        }

        db.TrackerPreferences.Add(
            new()
            {
                UserId = guest,
                TargetMg = 18
            });

        db.NotificationPreferences.Add(
            new()
            {
                UserId = guest,
                SleepCheckIn = true
            });

        db.BrowserPushSubscriptions.Add(
            new()
            {
                UserId = guest,

                Endpoint =
                    "https://fcm.googleapis.com/fcm/send/x",

                EndpointHash =
                    "test-only"
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

        var sleeps =
            await db.SleepLogs
                .OrderBy(
                    s => s.Id)
                .ToListAsync();

        Assert.Equal(
            2,
            sleeps.Count);

        Assert.All(
            sleeps,
            s =>
                Assert.Equal(
                    "42",
                    s.UserId));

        Assert.Equal(
            guest,
            sleeps[0].Notes);

        Assert.True(
            sleeps[0].IsImportedDuplicate);

        Assert.False(
            sleeps[1].IsImportedDuplicate);

        Assert.Equal(
            18,
            (
                await db.TrackerPreferences
                    .SingleAsync()
            ).TargetMg);

        Assert.Equal(
            "42",
            (
                await db.NotificationPreferences
                    .SingleAsync()
            ).UserId);

        Assert.Equal(
            "42",
            (
                await db.BrowserPushSubscriptions
                    .SingleAsync()
            ).UserId);

        await repo.DeleteAllLogsForUserAsync(
            "42");

        Assert.Empty(
            await db.SleepLogs
                .ToListAsync());

        Assert.Empty(
            await db.TrackerPreferences
                .ToListAsync());

        Assert.Empty(
            await db.NotificationPreferences
                .ToListAsync());

        Assert.Empty(
            await db.BrowserPushSubscriptions
                .ToListAsync());
    }
}