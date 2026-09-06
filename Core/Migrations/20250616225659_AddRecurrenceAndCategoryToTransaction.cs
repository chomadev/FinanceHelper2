using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceHelper.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurrenceAndCategoryToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "categoria_sugerida",
                table: "transacoes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_recorrente",
                table: "transacoes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "categoria_sugerida",
                table: "transacoes");

            migrationBuilder.DropColumn(
                name: "is_recorrente",
                table: "transacoes");
        }
    }
}
