using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace Caffeine.Migrations
{
    /// <inheritdoc/>
    public partial class Koffi50SleepMailing : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(name: "EmailConfirmed", table: "Users", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<bool>(name: "EmailVerificationRequired", table: "Users", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.AddColumn<DateTime>(name: "SleepEndedUtc", table: "SleepLogs", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "SleepStartedUtc", table: "SleepLogs", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<bool>(name: "IsArchived", table: "Beverages", type: "INTEGER", nullable: false, defaultValue: false);
            migrationBuilder.CreateTable(name: "EmailConfirmationTokens", columns: table => new { Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true), UserId = table.Column<int>(type: "INTEGER", nullable: false), TokenHash = table.Column<string>(type: "TEXT", nullable: false), Email = table.Column<string>(type: "TEXT", nullable: false), CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false), ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false), UsedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_EmailConfirmationTokens", x => x.Id);
                table.ForeignKey(name: "FK_EmailConfirmationTokens_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "GuestPromptPreferences", columns: table => new { UserId = table.Column<string>(type: "TEXT", nullable: false), DismissedUntil = table.Column<DateTime>(type: "TEXT", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_GuestPromptPreferences", x => x.UserId);
            });
            migrationBuilder.CreateTable(name: "MailCampaigns", columns: table => new { Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true), Key = table.Column<string>(type: "TEXT", nullable: false), ContentJson = table.Column<string>(type: "TEXT", nullable: false), Started = table.Column<bool>(type: "INTEGER", nullable: false), TestSentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true), CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_MailCampaigns", x => x.Id);
            });
            migrationBuilder.CreateTable(name: "MailingConsents", columns: table => new { Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true), UserId = table.Column<int>(type: "INTEGER", nullable: false), Kind = table.Column<string>(type: "TEXT", nullable: false), Enabled = table.Column<bool>(type: "INTEGER", nullable: false), AtUtc = table.Column<DateTime>(type: "TEXT", nullable: false), PolicyVersion = table.Column<string>(type: "TEXT", nullable: false) }, constraints: table =>
            {
                table.PrimaryKey("PK_MailingConsents", x => x.Id);
                table.ForeignKey(name: "FK_MailingConsents_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "MailingPreferences", columns: table => new { UserId = table.Column<int>(type: "INTEGER", nullable: false), Announcements = table.Column<bool>(type: "INTEGER", nullable: false), Weekly = table.Column<bool>(type: "INTEGER", nullable: false), Culture = table.Column<string>(type: "TEXT", nullable: false), AnnouncementVersion = table.Column<string>(type: "TEXT", nullable: false), WeeklyVersion = table.Column<string>(type: "TEXT", nullable: false), AnnouncementConsentUtc = table.Column<DateTime>(type: "TEXT", nullable: true), WeeklyConsentUtc = table.Column<DateTime>(type: "TEXT", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_MailingPreferences", x => x.UserId);
                table.ForeignKey(name: "FK_MailingPreferences_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateTable(name: "MailJobs", columns: table => new { Id = table.Column<int>(type: "INTEGER", nullable: false).Annotation("Sqlite:Autoincrement", true), UserId = table.Column<int>(type: "INTEGER", nullable: false), Kind = table.Column<string>(type: "TEXT", nullable: false), DeduplicationKey = table.Column<string>(type: "TEXT", nullable: false), ProtectedPayload = table.Column<string>(type: "TEXT", nullable: false), Culture = table.Column<string>(type: "TEXT", nullable: false), CampaignId = table.Column<int>(type: "INTEGER", nullable: true), WeekStart = table.Column<DateTime>(type: "TEXT", nullable: true), State = table.Column<string>(type: "TEXT", nullable: false), Attempts = table.Column<int>(type: "INTEGER", nullable: false), LeaseId = table.Column<string>(type: "TEXT", nullable: true), LeaseUntilUtc = table.Column<DateTime>(type: "TEXT", nullable: true), CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false), NextAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: false), SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true) }, constraints: table =>
            {
                table.PrimaryKey("PK_MailJobs", x => x.Id);
                table.ForeignKey(name: "FK_MailJobs_MailCampaigns_CampaignId", column: x => x.CampaignId, principalTable: "MailCampaigns", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "FK_MailJobs_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
            migrationBuilder.CreateIndex(name: "IX_EmailConfirmationTokens_TokenHash", table: "EmailConfirmationTokens", column: "TokenHash", unique: true);
            migrationBuilder.CreateIndex(name: "IX_EmailConfirmationTokens_UserId", table: "EmailConfirmationTokens", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_MailCampaigns_Key", table: "MailCampaigns", column: "Key", unique: true);
            migrationBuilder.CreateIndex(name: "IX_MailingConsents_UserId", table: "MailingConsents", column: "UserId");
            migrationBuilder.CreateIndex(name: "IX_MailJobs_CampaignId", table: "MailJobs", column: "CampaignId");
            migrationBuilder.CreateIndex(name: "IX_MailJobs_State_NextAttemptUtc", table: "MailJobs", columns: new[] { "State", "NextAttemptUtc" });
            migrationBuilder.CreateIndex(name: "IX_MailJobs_UserId_DeduplicationKey", table: "MailJobs", columns: new[] { "UserId", "DeduplicationKey" }, unique: true);
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EmailConfirmationTokens");
            migrationBuilder.DropTable(name: "GuestPromptPreferences");
            migrationBuilder.DropTable(name: "MailingConsents");
            migrationBuilder.DropTable(name: "MailingPreferences");
            migrationBuilder.DropTable(name: "MailJobs");
            migrationBuilder.DropTable(name: "MailCampaigns");
            migrationBuilder.DropColumn(name: "EmailConfirmed", table: "Users");
            migrationBuilder.DropColumn(name: "EmailVerificationRequired", table: "Users");
            migrationBuilder.DropColumn(name: "SleepEndedUtc", table: "SleepLogs");
            migrationBuilder.DropColumn(name: "SleepStartedUtc", table: "SleepLogs");
            migrationBuilder.DropColumn(name: "IsArchived", table: "Beverages");
        }
    }
}
