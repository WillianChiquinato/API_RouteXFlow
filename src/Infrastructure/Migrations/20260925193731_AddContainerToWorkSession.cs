using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContainerToWorkSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "container_id",
                table: "work_session",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_session_container_id",
                table: "work_session",
                column: "container_id");

            migrationBuilder.AddForeignKey(
                name: "FK_work_session_container_container_id",
                table: "work_session",
                column: "container_id",
                principalTable: "container",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_work_session_container_container_id",
                table: "work_session");

            migrationBuilder.DropIndex(
                name: "IX_work_session_container_id",
                table: "work_session");

            migrationBuilder.DropColumn(
                name: "container_id",
                table: "work_session");
        }
    }
}
