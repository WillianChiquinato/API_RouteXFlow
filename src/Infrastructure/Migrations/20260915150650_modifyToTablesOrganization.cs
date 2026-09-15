using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class modifyToTablesOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_route_route_id",
                table: "deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_route_evaluation_route_route_id",
                table: "route_evaluation");

            migrationBuilder.DropTable(
                name: "route");

            migrationBuilder.DropColumn(
                name: "description",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "name",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "description",
                table: "container");

            migrationBuilder.RenameColumn(
                name: "route_id",
                table: "route_evaluation",
                newName: "delivery_offer_id");

            migrationBuilder.RenameIndex(
                name: "IX_route_evaluation_route_id",
                table: "route_evaluation",
                newName: "IX_route_evaluation_delivery_offer_id");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "device",
                newName: "device_identifier");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "delivery_stop",
                newName: "longitude");

            migrationBuilder.RenameColumn(
                name: "route_id",
                table: "deliveries",
                newName: "status_id");

            migrationBuilder.RenameColumn(
                name: "delivery_date",
                table: "deliveries",
                newName: "started_at");

            migrationBuilder.RenameIndex(
                name: "IX_deliveries_route_id",
                table: "deliveries",
                newName: "IX_deliveries_status_id");

            migrationBuilder.AddColumn<decimal>(
                name: "additional_distance_km",
                table: "route_evaluation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "additional_time_minutes",
                table: "route_evaluation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "recommended",
                table: "route_evaluation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "route_deviation_km",
                table: "route_evaluation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "value_per_km",
                table: "route_evaluation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "delivery_stop",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "latitude",
                table: "delivery_stop",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "sequence",
                table: "delivery_stop",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "type",
                table: "delivery_stop",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "app_id",
                table: "delivery_offer",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "bonus_value",
                table: "delivery_offer",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "detected_at",
                table: "delivery_offer",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "estimated_minutes",
                table: "delivery_offer",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "raw_data",
                table: "delivery_offer",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "status_id",
                table: "delivery_offer",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "total_distance_km",
                table: "delivery_offer",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "value",
                table: "delivery_offer",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "accepted_at",
                table: "deliveries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "actual_distance_km",
                table: "deliveries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "actual_duration_minutes",
                table: "deliveries",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "completed_at",
                table: "deliveries",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "delivery_offer_id",
                table: "deliveries",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "container_device",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_connected_at",
                table: "container_device",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "paired_at",
                table: "container_device",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "container",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "apps",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    icon_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apps", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_status", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_offer_app_id",
                table: "delivery_offer",
                column: "app_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_offer_status_id",
                table: "delivery_offer",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_deliveries_delivery_offer_id",
                table: "deliveries",
                column: "delivery_offer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_delivery_offer_delivery_offer_id",
                table: "deliveries",
                column: "delivery_offer_id",
                principalTable: "delivery_offer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_status_status_id",
                table: "deliveries",
                column: "status_id",
                principalTable: "status",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_offer_apps_app_id",
                table: "delivery_offer",
                column: "app_id",
                principalTable: "apps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_delivery_offer_status_status_id",
                table: "delivery_offer",
                column: "status_id",
                principalTable: "status",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_route_evaluation_delivery_offer_delivery_offer_id",
                table: "route_evaluation",
                column: "delivery_offer_id",
                principalTable: "delivery_offer",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_delivery_offer_delivery_offer_id",
                table: "deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_deliveries_status_status_id",
                table: "deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_delivery_offer_apps_app_id",
                table: "delivery_offer");

            migrationBuilder.DropForeignKey(
                name: "FK_delivery_offer_status_status_id",
                table: "delivery_offer");

            migrationBuilder.DropForeignKey(
                name: "FK_route_evaluation_delivery_offer_delivery_offer_id",
                table: "route_evaluation");

            migrationBuilder.DropTable(
                name: "apps");

            migrationBuilder.DropTable(
                name: "status");

            migrationBuilder.DropIndex(
                name: "IX_delivery_offer_app_id",
                table: "delivery_offer");

            migrationBuilder.DropIndex(
                name: "IX_delivery_offer_status_id",
                table: "delivery_offer");

            migrationBuilder.DropIndex(
                name: "IX_deliveries_delivery_offer_id",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "additional_distance_km",
                table: "route_evaluation");

            migrationBuilder.DropColumn(
                name: "additional_time_minutes",
                table: "route_evaluation");

            migrationBuilder.DropColumn(
                name: "recommended",
                table: "route_evaluation");

            migrationBuilder.DropColumn(
                name: "route_deviation_km",
                table: "route_evaluation");

            migrationBuilder.DropColumn(
                name: "value_per_km",
                table: "route_evaluation");

            migrationBuilder.DropColumn(
                name: "address",
                table: "delivery_stop");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "delivery_stop");

            migrationBuilder.DropColumn(
                name: "sequence",
                table: "delivery_stop");

            migrationBuilder.DropColumn(
                name: "type",
                table: "delivery_stop");

            migrationBuilder.DropColumn(
                name: "app_id",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "bonus_value",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "detected_at",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "estimated_minutes",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "raw_data",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "total_distance_km",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "value",
                table: "delivery_offer");

            migrationBuilder.DropColumn(
                name: "accepted_at",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "actual_distance_km",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "actual_duration_minutes",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "delivery_offer_id",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "container_device");

            migrationBuilder.DropColumn(
                name: "last_connected_at",
                table: "container_device");

            migrationBuilder.DropColumn(
                name: "paired_at",
                table: "container_device");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "container");

            migrationBuilder.RenameColumn(
                name: "delivery_offer_id",
                table: "route_evaluation",
                newName: "route_id");

            migrationBuilder.RenameIndex(
                name: "IX_route_evaluation_delivery_offer_id",
                table: "route_evaluation",
                newName: "IX_route_evaluation_route_id");

            migrationBuilder.RenameColumn(
                name: "device_identifier",
                table: "device",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "longitude",
                table: "delivery_stop",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "status_id",
                table: "deliveries",
                newName: "route_id");

            migrationBuilder.RenameColumn(
                name: "started_at",
                table: "deliveries",
                newName: "delivery_date");

            migrationBuilder.RenameIndex(
                name: "IX_deliveries_status_id",
                table: "deliveries",
                newName: "IX_deliveries_route_id");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "deliveries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "deliveries",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "container",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "route",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    end_location = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    start_location = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route", x => x.id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_deliveries_route_route_id",
                table: "deliveries",
                column: "route_id",
                principalTable: "route",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_route_evaluation_route_route_id",
                table: "route_evaluation",
                column: "route_id",
                principalTable: "route",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
