using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace Caffeine.Migrations
{

    public partial class _20 : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Beverages",
                columns: new[] { "Id", "CaffeinePer100Ml", "Category", "DefaultPortionMl", "Name" },
                values: new object[,]
                {
                    { 35, 15.199999999999999, "Soda", 330, "Mountain Dew" },
                    { 36, 33.299999999999997, "Capsule", 150, "Dolce Gusto (Iced Frappé)" },
                    { 37, 36.100000000000001, "Capsule", 180, "Dolce Gusto (Flat White)" },
                    { 38, 37.5, "Capsule", 200, "Dolce Gusto (Starbucks Caramel Macchiato)" }
                });
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 38);
        }
    }
}
