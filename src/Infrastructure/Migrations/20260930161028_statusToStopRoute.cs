using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class statusToStopRoute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status_stop",
                table: "route_position_stop",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_route_position_stop_status_stop",
                table: "route_position_stop",
                column: "status_stop");

            migrationBuilder.AddForeignKey(
                name: "FK_route_position_stop_status_status_stop",
                table: "route_position_stop",
                column: "status_stop",
                principalTable: "status",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_route_position_stop_status_status_stop",
                table: "route_position_stop");

            migrationBuilder.DropIndex(
                name: "IX_route_position_stop_status_stop",
                table: "route_position_stop");

            migrationBuilder.DropColumn(
                name: "status_stop",
                table: "route_position_stop");
        }
    }
}
