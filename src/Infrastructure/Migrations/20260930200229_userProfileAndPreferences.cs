using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class userProfileAndPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "date_format",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "dd/MM/yyyy");

            migrationBuilder.AddColumn<string>(
                name: "distance_unit",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "km");

            migrationBuilder.AddColumn<decimal>(
                name: "fuel_price_per_liter",
                table: "user",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fuel_unit",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "l");

            migrationBuilder.AddColumn<string>(
                name: "time_format",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "24h");

            migrationBuilder.AddColumn<string>(
                name: "time_zone",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "America/Sao_Paulo");

            migrationBuilder.AddColumn<decimal>(
                name: "vehicle_km_per_liter",
                table: "user",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vehicle_name",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "date_format",
                table: "user");

            migrationBuilder.DropColumn(
                name: "distance_unit",
                table: "user");

            migrationBuilder.DropColumn(
                name: "fuel_price_per_liter",
                table: "user");

            migrationBuilder.DropColumn(
                name: "fuel_unit",
                table: "user");

            migrationBuilder.DropColumn(
                name: "time_format",
                table: "user");

            migrationBuilder.DropColumn(
                name: "time_zone",
                table: "user");

            migrationBuilder.DropColumn(
                name: "vehicle_km_per_liter",
                table: "user");

            migrationBuilder.DropColumn(
                name: "vehicle_name",
                table: "user");
        }
    }
}
