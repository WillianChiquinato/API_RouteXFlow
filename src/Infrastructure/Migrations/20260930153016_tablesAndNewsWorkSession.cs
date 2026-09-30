using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class tablesAndNewsWorkSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gps_position_work_session_worksession_id",
                table: "gps_position");

            migrationBuilder.DropIndex(
                name: "IX_gps_position_worksession_id",
                table: "gps_position");

            migrationBuilder.DropColumn(
                name: "worksession_id",
                table: "gps_position");

            migrationBuilder.AlterColumn<string>(
                name: "longitude",
                table: "gps_position",
                type: "text",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AlterColumn<string>(
                name: "latitude",
                table: "gps_position",
                type: "text",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision");

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "gps_position",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "route_position",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type = table.Column<int>(type: "integer", nullable: false),
                    worksession_id = table.Column<int>(type: "integer", nullable: false),
                    origin_gps_position_id = table.Column<int>(type: "integer", nullable: false),
                    destination_gps_position_id = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_position", x => x.id);
                    table.ForeignKey(
                        name: "FK_route_position_gps_position_destination_gps_position_id",
                        column: x => x.destination_gps_position_id,
                        principalTable: "gps_position",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_route_position_gps_position_origin_gps_position_id",
                        column: x => x.origin_gps_position_id,
                        principalTable: "gps_position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_route_position_work_session_worksession_id",
                        column: x => x.worksession_id,
                        principalTable: "work_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_position_stop",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    route_position_id = table.Column<int>(type: "integer", nullable: false),
                    gps_position_id = table.Column<int>(type: "integer", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_route_position_stop", x => x.id);
                    table.ForeignKey(
                        name: "FK_route_position_stop_gps_position_gps_position_id",
                        column: x => x.gps_position_id,
                        principalTable: "gps_position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_route_position_stop_route_position_route_position_id",
                        column: x => x.route_position_id,
                        principalTable: "route_position",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_route_position_destination_gps_position_id",
                table: "route_position",
                column: "destination_gps_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_position_origin_gps_position_id",
                table: "route_position",
                column: "origin_gps_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_position_worksession_id",
                table: "route_position",
                column: "worksession_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_position_stop_gps_position_id",
                table: "route_position_stop",
                column: "gps_position_id");

            migrationBuilder.CreateIndex(
                name: "IX_route_position_stop_route_position_id",
                table: "route_position_stop",
                column: "route_position_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "route_position_stop");

            migrationBuilder.DropTable(
                name: "route_position");

            migrationBuilder.DropColumn(
                name: "address",
                table: "gps_position");

            migrationBuilder.AlterColumn<double>(
                name: "longitude",
                table: "gps_position",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<double>(
                name: "latitude",
                table: "gps_position",
                type: "double precision",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "worksession_id",
                table: "gps_position",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_gps_position_worksession_id",
                table: "gps_position",
                column: "worksession_id");

            migrationBuilder.AddForeignKey(
                name: "FK_gps_position_work_session_worksession_id",
                table: "gps_position",
                column: "worksession_id",
                principalTable: "work_session",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
