using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class dBSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "apps",
                columns: new[] { "id", "created_at", "description", "icon_url", "name", "type_app", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Aplicativo Vermelho de entregas", "", "Ifood", 1, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Aplicativo Amarelo de entregas", "", "99Food", 1, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Aplicativo Verde e Amarelo de entregas", "", "Keeta", 1, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Aplicativo vermelho de entregas", "", "Shoppe", 2, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Aplicativo Amarelo de entregas", "", "Mercado Livre", 2, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "role",
                columns: new[] { "id", "created_at", "description", "name", "updated_at" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Perfil feito para Desenvolvedores e administradores gerais", "Super-Admin", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Perfil feito para administradores comuns", "Admin", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Perfil para operadores", "Operação", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "status",
                columns: new[] { "id", "created_at", "name", "updated_at" },
                values: new object[,]
                {
                    { 11, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Ativo", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 12, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Inativo", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 13, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Suspenso", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 14, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Aprovado", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 15, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Reprovado", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 16, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Em Análise", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 17, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Pendente", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 18, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Em Andamento", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 19, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Concluído", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) },
                    { 20, new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc), "Cancelado", new DateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "apps",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "apps",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "apps",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "apps",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "apps",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "role",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "status",
                keyColumn: "id",
                keyValue: 20);
        }
    }
}
