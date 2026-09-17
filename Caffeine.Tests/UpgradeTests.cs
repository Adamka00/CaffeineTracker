using Caffeine.Data;
using Caffeine.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Caffeine.Tests;

public class UpgradeTests
{
    [Fact]
    public async Task PopulatedReleasedSchemaRetainsEveryLogUserAndCaffeineSnapshot()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var db =
            new AppDbContext(options);

        await db
            .GetService<IMigrator>()
            .MigrateAsync("20260821170905_3.0");


        // Reproduce the deployed but uncommitted account schema.
        await db.Database.ExecuteSqlRawAsync("""
            ALTER TABLE CaffeineLogs
            ADD COLUMN UserId TEXT NOT NULL DEFAULT '';

            CREATE TABLE Users (
                Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                Email TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );

            INSERT INTO Users
            VALUES (
                8,
                'Existing user',
                'existing@example.test',
                'hash-must-not-change',
                '2026-08-22 10:00:00'
            );

            INSERT INTO Beverages (
                Id,
                Name,
                Category,
                CaffeinePer100Ml,
                DefaultPortionMl
            )
            VALUES (
                100,
                'Legacy custom',
                'Custom',
                38.4,
                250
            );

            INSERT INTO CaffeineLogs (
                Id,
                BeverageId,
                ConsumedAt,
                ConsumedAmountMl,
                TotalCaffeineMg,
                UserId
            )
            VALUES
                (
                    50,
                    100,
                    '2026-09-16 10:15:00',
                    250,
                    96,
                    '8'
                ),
                (
                    51,
                    100,
                    '2026-09-16 11:00:00',
                    500,
                    191.7,
                    'Guest_11111111-1111-4111-8111-111111111111'
                ),
                (
                    52,
                    3,
                    '2026-09-15 12:00:00',
                    500,
                    157.2,
                    '8'
                );
            """);


        await DatabaseUpgrade.ApplyAsync(db);

        db.ChangeTracker.Clear();


        var entries =
            await db.CaffeineLogs
                .Include(l => l.Beverage)
                .OrderBy(l => l.Id)
                .ToListAsync();


        Assert.Equal(
            new[] { 50, 51, 52 },
            entries.Select(l => l.Id));

        Assert.Equal(
            new[] { 96.0, 191.7, 157.2 },
            entries.Select(l => l.TotalCaffeineMg));

        Assert.Equal(
            new[] { 250, 500, 500 },
            entries.Select(l => l.ConsumedAmountMl));


        Assert.Equal(
            new DateTime(2026, 9, 16, 10, 15, 0),
            entries[0].ConsumedAt);


        Assert.Equal(
            "8",
            entries[0].UserId);

        Assert.Equal(
            "8",
            entries[0].Beverage.OwnerId);


        Assert.Equal(
            entries[1].UserId,
            entries[1].Beverage.OwnerId);


        Assert.NotEqual(
            entries[0].BeverageId,
            entries[1].BeverageId);


        Assert.Equal(
            "Legacy custom",
            entries[0].Beverage.Name);


        Assert.Equal(
            "hash-must-not-change",
            (await db.Users.SingleAsync()).PasswordHash);

        Assert.Equal(
            8,
            (await db.Users.SingleAsync()).Id);


        Assert.NotNull(
            await db.Beverages.FindAsync(100));


        Assert.False(
            db.Database.HasPendingModelChanges());


        var drinks =
            await db.Beverages.CountAsync();


        // Az upgrade másodszori futtatása sem duplikálhat
        // vagy ronthat el adatokat.
        await DatabaseUpgrade.ApplyAsync(db);


        Assert.Equal(
            drinks,
            await db.Beverages.CountAsync());

        Assert.Equal(
            3,
            await db.CaffeineLogs.CountAsync());
    }
}