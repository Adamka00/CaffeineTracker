using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace Caffeine.Migrations
{

    public partial class InitialCreate : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beverages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: false),
                    CaffeinePer100Ml = table.Column<double>(type: "REAL", nullable: false),
                    DefaultPortionMl = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beverages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CaffeineLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConsumedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConsumedAmountMl = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalCaffeineMg = table.Column<double>(type: "REAL", nullable: false),
                    BeverageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaffeineLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaffeineLogs_Beverages_BeverageId",
                        column: x => x.BeverageId,
                        principalTable: "Beverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Beverages",
                columns: new[] { "Id", "CaffeinePer100Ml", "Category", "DefaultPortionMl", "Name" },
                values: new object[,]
                {
                    { 1, 32.0, "Energy Drink", 250, "Red Bull (Classic / Sugarfree)" },
                    { 2, 32.0, "Energy Drink", 250, "Red Bull Kókusz-Áfonya (Edition)" },
                    { 3, 32.0, "Energy Drink", 500, "Monster Energy (Original)" },
                    { 4, 30.0, "Energy Drink", 500, "Monster Ultra (Fehér/Zero)" },
                    { 5, 32.0, "Energy Drink", 500, "Monster Mango Loco" },
                    { 6, 32.0, "Energy Drink", 250, "Hell Energy Classic" },
                    { 7, 32.0, "Energy Drink", 250, "Hell Energy Zero" },
                    { 8, 32.0, "Energy Drink", 250, "Burn Original" },
                    { 9, 32.0, "Energy Drink", 250, "Bomba! Classic" },
                    { 10, 38.399999999999999, "Energy Drink", 250, "Hell Strong (Apple / Focus)" },
                    { 11, 38.399999999999999, "Energy Drink", 250, "Hell Strong Watermelon" },
                    { 12, 40.0, "Energy Drink", 500, "Reign Total Body Fuel" },
                    { 13, 40.0, "Ice Coffee", 250, "Hell Ice Coffee Latte / Cappuccino" },
                    { 14, 40.0, "Ice Coffee", 250, "Hell Ice Coffee Double Espresso" },
                    { 15, 40.0, "Ice Coffee", 250, "Starbucks Frappuccino (üveges)" },
                    { 16, 25.0, "Ice Coffee", 330, "Mizo Kávé (dobozos)" },
                    { 17, 212.0, "Coffee", 30, "Espresso (Kávézós)" },
                    { 18, 212.0, "Coffee", 60, "Dupla Espresso" },
                    { 19, 60.0, "Coffee", 120, "Hosszú Kávé (Lungo)" },
                    { 20, 30.0, "Coffee", 200, "Cappuccino" },
                    { 21, 40.0, "Coffee", 250, "Filteres kávé (Bögre)" },
                    { 22, 30.0, "Coffee", 200, "Instant Kávé (Nescafé, 1 bögre)" },
                    { 23, 162.5, "Capsule", 40, "Nespresso (Original Espresso kapszula)" },
                    { 24, 75.0, "Capsule", 110, "Nespresso (Original Lungo kapszula)" },
                    { 25, 175.0, "Capsule", 40, "Dolce Gusto (Espresso kapszula)" },
                    { 26, 83.0, "Capsule", 120, "Dolce Gusto (Lungo / Grande kapszula)" },
                    { 27, 9.5999999999999996, "Soda", 330, "Coca-Cola (Classic / Zero)" },
                    { 28, 10.9, "Soda", 330, "Pepsi" },
                    { 29, 12.800000000000001, "Soda", 330, "Pepsi Max" },
                    { 30, 11.4, "Soda", 330, "Dr Pepper" },
                    { 31, 20.0, "Tea", 250, "Fekete Tea (Bögre)" },
                    { 32, 12.0, "Tea", 250, "Zöld Tea (Bögre)" },
                    { 33, 35.0, "Tea", 250, "Yerba Mate" },
                    { 34, 70.0, "Tea", 100, "Matcha (Tradicionális elkészítés)" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaffeineLogs_BeverageId",
                table: "CaffeineLogs",
                column: "BeverageId");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaffeineLogs");

            migrationBuilder.DropTable(
                name: "Beverages");
        }
    }
}
