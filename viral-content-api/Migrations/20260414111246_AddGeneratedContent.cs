using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace viral_content_api.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingEventLogs_Users_UserId",
                table: "BillingEventLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId1",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_ProcessedWebhookEvents_Provider_ExternalEventId",
                table: "ProcessedWebhookEvents");

            migrationBuilder.DropIndex(
                name: "IX_Posts_UserId1",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_BillingEventLogs_CreatedAtUtc",
                table: "BillingEventLogs");

            migrationBuilder.DropIndex(
                name: "IX_AiUsageRecords_UserId_UsageDateUtc",
                table: "AiUsageRecords");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Posts");

            migrationBuilder.CreateTable(
                name: "GeneratedContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Topic = table.Column<string>(type: "TEXT", nullable: false),
                    Platform = table.Column<string>(type: "TEXT", nullable: false),
                    Tone = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GeneratedContents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageRecords_UserId",
                table: "AiUsageRecords",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingEventLogs_Users_UserId",
                table: "BillingEventLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingEventLogs_Users_UserId",
                table: "BillingEventLogs");

            migrationBuilder.DropTable(
                name: "GeneratedContents");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_AiUsageRecords_UserId",
                table: "AiUsageRecords");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Posts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessedWebhookEvents_Provider_ExternalEventId",
                table: "ProcessedWebhookEvents",
                columns: new[] { "Provider", "ExternalEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId1",
                table: "Posts",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_BillingEventLogs_CreatedAtUtc",
                table: "BillingEventLogs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AiUsageRecords_UserId_UsageDateUtc",
                table: "AiUsageRecords",
                columns: new[] { "UserId", "UsageDateUtc" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BillingEventLogs_Users_UserId",
                table: "BillingEventLogs",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId1",
                table: "Posts",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
