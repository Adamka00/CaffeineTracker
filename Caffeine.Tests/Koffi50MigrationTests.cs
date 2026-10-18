using System.Text.Json;
using Caffeine.Data;
using Caffeine.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Xunit;

namespace Caffeine.Tests;
public sealed class Koffi50MigrationTests
{
    private sealed record TableSnapshot(string[] Columns, string Rows);
    private static async Task<Dictionary<string, TableSnapshot>> Snapshot(AppDbContext db, Dictionary<string, TableSnapshot>? projection = null)
    {
        await db.Database.OpenConnectionAsync();
        var conn = db.Database.GetDbConnection();
        var tables = new List<string>();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory' ORDER BY name";
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));
        }

        var result = new Dictionary<string, TableSnapshot>();
        foreach (var table in tables)
        {
            if (projection != null && !projection.ContainsKey(table))
                continue;
            using var cmd = conn.CreateCommand();
            var select = projection == null ? "*" : string.Join(",", projection[table].Columns.Select(c => "\"" + c + "\""));
            cmd.CommandText = "SELECT " + select + " FROM \"" + table + "\" ORDER BY rowid";
            await using var reader = await cmd.ExecuteReaderAsync();
            var columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
            var rows = new List<string[]>();
            while (await reader.ReadAsync())
                rows.Add(Enumerable.Range(0, reader.FieldCount).Select(i => reader.IsDBNull(i) ? "<NULL>" : reader.GetValue(i).ToString()!).ToArray());
            result[table] = new(columns, JsonSerializer.Serialize(rows));
        }

        return result;
    }

    [Fact]
    public async Task UpgradePopulated40KeepsEveryOriginalColumnAndDefaultsNewFieldsSafely()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        await db.GetService<IMigrator>().MigrateAsync("20260821170905_3.0");
        await db.Database.ExecuteSqlRawAsync("""
   ALTER TABLE CaffeineLogs ADD COLUMN UserId TEXT NOT NULL DEFAULT '';
   CREATE TABLE Users (Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,Username TEXT NOT NULL,Email TEXT NOT NULL,PasswordHash TEXT NOT NULL,CreatedAt TEXT NOT NULL);
   INSERT INTO __EFMigrationsHistory(MigrationId,ProductVersion)VALUES('20260827170851_AddUserProfileSystem','10.0.11');
   """);
        await db.GetService<IMigrator>().MigrateAsync("20261005171337_Koffi40Experience");
        await db.Database.ExecuteSqlRawAsync("""
   INSERT INTO Users(Id,Username,Email,PasswordHash,CreatedAt,SecurityStamp)VALUES(42,'existing','old@example.test','keep-hash','2026-09-01','keep-stamp');
   INSERT INTO Beverages(Id,OwnerId,Name,Category,CaffeinePer100Ml,DefaultPortionMl)VALUES(200,'42','Own','Custom',32,500);
   INSERT INTO CaffeineLogs(UserId,BeverageId,ConsumedAt,ConsumedAmountMl,TotalCaffeineMg)VALUES('42',200,'2026-10-01 12:00:00',500,160);
   INSERT INTO CaffeineLogs(UserId,BeverageId,ConsumedAt,ConsumedAmountMl,TotalCaffeineMg)VALUES('Guest_11111111-1111-4111-8111-111111111111',4,'2026-10-02 12:00:00',500,159.9);
   INSERT INTO FavoriteDrinks(UserId,BeverageId,AmountMl)VALUES('42',200,500);
   INSERT INTO SleepLogs(UserId,SleepDate,ActualBedtime,WakeTime,SleepRating,FallingAsleep,Awakenings,EstimatedCaffeineAtBedtime,PreviousDayCaffeineMg,IsImportedDuplicate,Notes)VALUES('42','2026-10-02 00:00:00','2026-10-01 23:30:00','2026-10-02 07:00:00',4,2,1,35,160,0,'keep notes');
   INSERT INTO TrackerPreferences(UserId,PlannedBedtime,TargetMg)VALUES('42','23:30',30);
   INSERT INTO ExperiencePreferences(UserId,TutorialCompleted,LastSeenRelease,MorningDismissedDate)VALUES('42',1,'4.0','2026-10-01');
   INSERT INTO CaffeineFreeDays(UserId,Day)VALUES('42','2026-10-03');
   """);
        var before = await Snapshot(db);
        await DatabaseUpgrade.ApplyAsync(db);
        await DatabaseUpgrade.ApplyAsync(db);
        var after = await Snapshot(db, before);
        Assert.Equal(before.Keys, after.Keys);
        foreach (var table in before)
        {
            Assert.Equal(table.Value.Columns, after[table.Key].Columns);
            Assert.Equal(table.Value.Rows, after[table.Key].Rows);
        }

        var user = await db.Users.SingleAsync();
        Assert.False(user.EmailConfirmed);
        Assert.False(user.EmailVerificationRequired);
        Assert.Equal("keep-hash", user.PasswordHash);
        Assert.Equal("keep-stamp", user.SecurityStamp);
        var sleep = await db.SleepLogs.SingleAsync();
        Assert.Null(sleep.SleepStartedUtc);
        Assert.Null(sleep.SleepEndedUtc);
        Assert.Equal(7.5, sleep.DurationHours);
        Assert.False((await db.Beverages.SingleAsync(b => b.Id == 200)).IsArchived);
        Assert.Empty(await db.MailingPreferences.ToListAsync());
        Assert.False(db.Database.HasPendingModelChanges());
    }
}
