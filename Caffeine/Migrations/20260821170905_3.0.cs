using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caffeine.Migrations
{

    public partial class _30 : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CaffeinePer100Ml",
                value: 32.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 14,
                column: "CaffeinePer100Ml",
                value: 48.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 15,
                column: "CaffeinePer100Ml",
                value: 30.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 25,
                column: "CaffeinePer100Ml",
                value: 200.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 36,
                column: "CaffeinePer100Ml",
                value: 53.299999999999997);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 37,
                column: "CaffeinePer100Ml",
                value: 55.5);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 38,
                column: "CaffeinePer100Ml",
                value: 40.0);
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 4,
                column: "CaffeinePer100Ml",
                value: 30.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 14,
                column: "CaffeinePer100Ml",
                value: 40.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 15,
                column: "CaffeinePer100Ml",
                value: 40.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 25,
                column: "CaffeinePer100Ml",
                value: 175.0);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 36,
                column: "CaffeinePer100Ml",
                value: 33.299999999999997);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 37,
                column: "CaffeinePer100Ml",
                value: 36.100000000000001);

            migrationBuilder.UpdateData(
                table: "Beverages",
                keyColumn: "Id",
                keyValue: 38,
                column: "CaffeinePer100Ml",
                value: 37.5);
        }
    }
}
