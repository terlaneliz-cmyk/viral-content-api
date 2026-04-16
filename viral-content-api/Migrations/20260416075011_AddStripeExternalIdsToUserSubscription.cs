using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace viral_content_api.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeExternalIdsToUserSubscription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalCustomerId",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExternalSubscriptionId",
                table: "UserSubscriptions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalCustomerId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "ExternalSubscriptionId",
                table: "UserSubscriptions");
        }
    }
}
