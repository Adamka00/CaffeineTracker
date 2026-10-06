using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caffeine.Migrations
{
    public partial class Koffi40Experience : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeverageBarcodes",
                columns: table => new
                {
                    Barcode = table.Column<string>(type: "TEXT", nullable: false),
                    BeverageId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeverageBarcodes", x => x.Barcode);
                    table.ForeignKey(
                        name: "FK_BeverageBarcodes_Beverages_BeverageId",
                        column: x => x.BeverageId,
                        principalTable: "Beverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExperiencePreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    TutorialCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastSeenRelease = table.Column<string>(type: "TEXT", nullable: false),
                    MorningDismissedDate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperiencePreferences", x => x.UserId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeverageBarcodes_BeverageId",
                table: "BeverageBarcodes",
                column: "BeverageId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeverageBarcodes");

            migrationBuilder.DropTable(
                name: "ExperiencePreferences");
        }
    }
}