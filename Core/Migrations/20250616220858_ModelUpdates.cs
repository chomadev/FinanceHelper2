using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceHelper.Core.Migrations
{
    /// <inheritdoc />
    public partial class ModelUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "transacoes",
                newName: "Id");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "data",
                table: "transacoes",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<Guid>(
                name: "identificador",
                table: "transacoes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "identificador",
                table: "transacoes");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "transacoes",
                newName: "id");

            migrationBuilder.AlterColumn<DateTime>(
                name: "data",
                table: "transacoes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");
        }
    }
}
