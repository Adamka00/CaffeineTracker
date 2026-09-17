using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caffeine.Migrations
{
    /// <inheritdoc />
    public partial class HistoryFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Beverages",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FavoriteDrinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    BeverageId = table.Column<int>(type: "INTEGER", nullable: false),
                    AmountMl = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteDrinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteDrinks_Beverages_BeverageId",
                        column: x => x.BeverageId,
                        principalTable: "Beverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 1,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 2,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 3,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 4,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 5,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 6,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 7,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 8,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 9,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 10,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 11,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 12,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 13,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 14,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 15,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 16,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 17,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 18,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 19,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 20,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 21,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 22,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 23,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 24,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 25,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 26,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 27,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 28,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 29,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 30,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 31,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 32,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 33,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 34,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 35,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 36,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 37,
                column: "OwnerId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 38,
                column: "OwnerId",
                value: null);

            // Legacy custom beverages were shared. Give each existing consumer a private
            // copy and preserve their log's caffeine snapshot. Do not guess the creator.
            migrationBuilder.Sql("""
                CREATE TEMP TABLE LegacyDrinkCopies AS
                SELECT b.Id AS OldId, l.UserId AS OwnerId,
                    (SELECT COALESCE(MAX(Id), 0) FROM Beverages) +
                    ROW_NUMBER() OVER (ORDER BY b.Id, l.UserId) AS NewId
                FROM Beverages b JOIN CaffeineLogs l ON l.BeverageId = b.Id
                WHERE b.Category = 'Custom' AND b.OwnerId IS NULL
                GROUP BY b.Id, l.UserId;
                INSERT INTO Beverages (Id, Name, Category, CaffeinePer100Ml, DefaultPortionMl, OwnerId)
                SELECT c.NewId, b.Name, b.Category, b.CaffeinePer100Ml, b.DefaultPortionMl, c.OwnerId
                FROM LegacyDrinkCopies c JOIN Beverages b ON b.Id = c.OldId;
                UPDATE CaffeineLogs SET BeverageId = (
                    SELECT c.NewId FROM LegacyDrinkCopies c
                    WHERE c.OldId = CaffeineLogs.BeverageId AND c.OwnerId = CaffeineLogs.UserId)
                WHERE EXISTS (SELECT 1 FROM LegacyDrinkCopies c
                    WHERE c.OldId = CaffeineLogs.BeverageId AND c.OwnerId = CaffeineLogs.UserId);
                DROP TABLE LegacyDrinkCopies;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_CaffeineLogs_UserId_ConsumedAt",
                table: "CaffeineLogs",
                columns: new[] { "UserId", "ConsumedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Beverages_OwnerId",
                table: "Beverages",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDrinks_BeverageId",
                table: "FavoriteDrinks",
                column: "BeverageId");

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteDrinks_UserId_BeverageId_AmountMl",
                table: "FavoriteDrinks",
                columns: new[] { "UserId", "BeverageId", "AmountMl" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteDrinks");

            migrationBuilder.DropIndex(
                name: "IX_CaffeineLogs_UserId_ConsumedAt",
                table: "CaffeineLogs");

            migrationBuilder.DropIndex(
                name: "IX_Beverages_OwnerId",
                table: "Beverages");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Beverages");
        }
    }
}