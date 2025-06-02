using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaAgendaCitas.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaCambioEstadoToCita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Citas",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCambioEstado",
                table: "Citas",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCambioEstado",
                table: "Citas");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Citas",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
