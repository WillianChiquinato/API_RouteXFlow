using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class preferencesToConfigs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "role",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "preferences",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    vehicle_name = table.Column<string>(type: "text", nullable: false),
                    vehicle_km_per_liter = table.Column<decimal>(type: "numeric", nullable: true),
                    fuel_price_per_liter = table.Column<decimal>(type: "numeric", nullable: true),
                    date_format = table.Column<string>(type: "text", nullable: false),
                    time_format = table.Column<string>(type: "text", nullable: false),
                    time_zone = table.Column<string>(type: "text", nullable: false),
                    distance_unit = table.Column<string>(type: "text", nullable: false),
                    fuel_unit = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_preferences", x => x.id);
                    table.ForeignKey(
                        name: "FK_preferences_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: 1,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: 2,
                column: "UserId",
                value: null);

            migrationBuilder.UpdateData(
                table: "role",
                keyColumn: "id",
                keyValue: 3,
                column: "UserId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_role_UserId",
                table: "role",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_preferences_user_id",
                table: "preferences",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_role_user_UserId",
                table: "role",
                column: "UserId",
                principalTable: "user",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_role_user_UserId",
                table: "role");

            migrationBuilder.DropTable(
                name: "preferences");

            migrationBuilder.DropIndex(
                name: "IX_role_UserId",
                table: "role");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "role");

            migrationBuilder.AddColumn<string>(
                name: "date_format",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "distance_unit",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");

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
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "time_format",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "time_zone",
                table: "user",
                type: "text",
                nullable: false,
                defaultValue: "");

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
    }
}
