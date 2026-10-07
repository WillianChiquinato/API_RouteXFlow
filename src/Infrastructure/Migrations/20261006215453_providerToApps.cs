using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class providerToApps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "provider_app_id",
                table: "oauth_integration",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_oauth_integration_provider_app_id",
                table: "oauth_integration",
                column: "provider_app_id");

            migrationBuilder.AddForeignKey(
                name: "FK_oauth_integration_apps_provider_app_id",
                table: "oauth_integration",
                column: "provider_app_id",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_oauth_integration_apps_provider_app_id",
                table: "oauth_integration");

            migrationBuilder.DropIndex(
                name: "IX_oauth_integration_provider_app_id",
                table: "oauth_integration");

            migrationBuilder.DropColumn(
                name: "provider_app_id",
                table: "oauth_integration");
        }
    }
}
