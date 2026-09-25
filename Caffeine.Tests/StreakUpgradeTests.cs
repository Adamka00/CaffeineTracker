using System.Text.Json;
using Caffeine.Data;
using Caffeine.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Caffeine.Tests;

public class StreakUpgradeTests
{
    [Fact]
    public async Task CurrentVersionUpgradePreservesEveryExistingTableValueAndIsRepeatable()
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


        await db.GetService<IMigrator>()
            .MigrateAsync(
                "20260821170905_3.0");


        await db.Database.ExecuteSqlRawAsync(
            """
            ALTER TABLE CaffeineLogs
            ADD COLUMN UserId TEXT NOT NULL DEFAULT '';

            CREATE TABLE Users (
                Id INTEGER NOT NULL
                    PRIMARY KEY AUTOINCREMENT,

                Username TEXT NOT NULL,
                Email TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
            """);


        await db.GetService<IMigrator>()
            .MigrateAsync(
                "20260917112735_HistoryFavorites");


        // Seed the OLD schema directly:
        // it intentionally has no SecurityStamp yet.
        await db.Database.ExecuteSqlRawAsync(
            """
            INSERT INTO Users
                (
                    Id,
                    Username,
                    Email,
                    PasswordHash,
                    CreatedAt
                )
            VALUES
                (
                    8,
                    'Existing',
                    'old@example.test',
                    'keep-hash',
                    '2026-08-29 11:22:33'
                );
            """);


        db.Beverages.Add(
            new()
            {
                Id = 123,
                Name = "My existing drink",
                Category = "Custom",
                OwnerId = "8",
                CaffeinePer100Ml = 38.4,
                DefaultPortionMl = 250
            });


        db.CaffeineLogs.Add(
            new()
            {
                Id = 456,
                UserId = "8",
                BeverageId = 123,

                ConsumedAt =
                    new DateTime(
                        2026,
                        9,
                        16,
                        11,
                        22,
                        33),

                TotalCaffeineMg = 95.7,
                ConsumedAmountMl = 250
            });


        db.FavoriteDrinks.Add(
            new()
            {
                Id = 789,
                UserId = "8",
                BeverageId = 123,
                AmountMl = 250
            });


        await db.SaveChangesAsync();


        var before =
            await Snapshot(connection);


        await DatabaseUpgrade.ApplyAsync(db);


        Assert.Equal(
            before,
            await Snapshot(connection));


        Assert.Empty(
            await db.CaffeineFreeDays
                .ToListAsync());


        Assert.Equal(
            "",
            (
                await db.Users
                    .SingleAsync()
            ).SecurityStamp);


        db.CaffeineFreeDays.Add(
            new()
            {
                UserId = "8",

                Day =
                    new DateTime(
                        2026,
                        9,
                        17)
            });


        await db.SaveChangesAsync();


        // Upgrade must be safe to run again.
        await DatabaseUpgrade.ApplyAsync(db);


        Assert.Equal(
            before,
            await Snapshot(connection));


        Assert.Single(
            await db.CaffeineFreeDays
                .ToListAsync());


        Assert.False(
            db.Database
                .HasPendingModelChanges());
    }


    private static async Task<string> Snapshot(
        SqliteConnection connection)
    {
        var tables =
            new Dictionary<
                string,
                List<object[]>>();


        foreach (
            var table in
            new[]
            {
                "Users",
                "Beverages",
                "CaffeineLogs",
                "FavoriteDrinks"
            })
        {
            using var command =
                connection.CreateCommand();


            // Compare every original column;
            // the newly added stamp is asserted separately.
            command.CommandText =
                table == "Users"
                    ? """
                      SELECT
                          Id,
                          Username,
                          Email,
                          PasswordHash,
                          CreatedAt
                      FROM Users
                      ORDER BY Id
                      """
                    : $"""
                       SELECT *
                       FROM {table}
                       ORDER BY Id
                       """;


            using var reader =
                await command
                    .ExecuteReaderAsync();


            var rows =
                new List<object[]>();


            while (
                await reader
                    .ReadAsync())
            {
                var values =
                    new object[
                        reader.FieldCount];


                reader.GetValues(values);

                rows.Add(values);
            }


            tables[table] = rows;
        }


        return JsonSerializer.Serialize(
            tables);
    }
}