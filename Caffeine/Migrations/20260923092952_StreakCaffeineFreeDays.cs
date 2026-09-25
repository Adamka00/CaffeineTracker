using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Caffeine.Migrations
{
    /// <inheritdoc />
    public partial class StreakCaffeineFreeDays : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaffeineFreeDays",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    Day = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaffeineFreeDays", x => new { x.UserId, x.Day });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaffeineFreeDays");
        }
    }
}
