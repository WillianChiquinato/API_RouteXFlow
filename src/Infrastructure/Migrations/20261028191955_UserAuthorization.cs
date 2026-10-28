using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserAuthorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "authorization_account_verify",
                table: "user",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "authorization_rejected_at",
                table: "user",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "authorization_rejection_reason",
                table: "user",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "email_verified_at",
                table: "user",
                type: "timestamp with time zone",
                nullable: true);

            // Contas anteriores a esta feature já estavam em uso: não podem cair na fila de aprovação.
            migrationBuilder.Sql("UPDATE \"user\" SET authorization_account_verify = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "authorization_account_verify",
                table: "user");

            migrationBuilder.DropColumn(
                name: "authorization_rejected_at",
                table: "user");

            migrationBuilder.DropColumn(
                name: "authorization_rejection_reason",
                table: "user");

            migrationBuilder.DropColumn(
                name: "email_verified_at",
                table: "user");
        }
    }
}
