using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Caffeine.Data;

// The released database had Users/UserId, but their migration was never committed.
// Reconcile that exact gap before applying the new migrations; keep existing IDs/data.
public static class DatabaseUpgrade
{
    public static async Task ApplyAsync(AppDbContext db)
    {
        const string legacyLast = "20260821170905_3.0";
        const string userProfileMigration = "20260827170851_AddUserProfileSystem";

        var applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();

        // First bring genuinely old databases up to the last known
        // pre-user-profile migration.
        if (!applied.Contains(legacyLast))
        {
            await db.GetService<IMigrator>()
                .MigrateAsync(legacyLast);
        }

        await db.Database.OpenConnectionAsync();

        try
        {
            var connection = db.Database.GetDbConnection();

            // Check whether the released database already contains UserId.
            var hasUserId = false;

            using (var schema = connection.CreateCommand())
            {
                schema.CommandText = "PRAGMA table_info(CaffeineLogs)";

                using var reader = await schema.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader.GetString(1) == "UserId")
                    {
                        hasUserId = true;
                        break;
                    }
                }
            }

            // Check whether the released database already contains Users.
            var hasUsersTable = false;

            using (var usersTable = connection.CreateCommand())
            {
                usersTable.CommandText = """
                    SELECT COUNT(*)
                    FROM sqlite_master
                    WHERE type = 'table'
                      AND name = 'Users';
                    """;

                hasUsersTable =
                    Convert.ToInt32(
                        await usersTable.ExecuteScalarAsync()) > 0;
            }

            applied = (await db.Database.GetAppliedMigrationsAsync()).ToList();

            var userProfileMigrationApplied =
                applied.Contains(userProfileMigration);

            // Special released-schema case:
            //
            // The production database already has the schema introduced by
            // AddUserProfileSystem, but its migration history entry is missing.
            //
            // Do NOT run that migration again. Instead, record it as applied.
            if (hasUserId &&
                hasUsersTable &&
                !userProfileMigrationApplied)
            {
                await db.Database.ExecuteSqlRawAsync(
                    """
                    INSERT OR IGNORE INTO __EFMigrationsHistory
                        (MigrationId, ProductVersion)
                    VALUES
                        ({0}, {1});
                    """,
                    userProfileMigration,
                    "10.0.11");
            }
        }
        finally
        {
            await db.Database.CloseConnectionAsync();
        }

        // From this point EF can safely apply every migration that comes
        // after AddUserProfileSystem.
        await db.Database.MigrateAsync();
    }
}