using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "continente",
                table: "usuario",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "unidad_temperatura",
                table: "usuario",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                defaultValue: "C");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "continente",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "unidad_temperatura",
                table: "usuario");
        }
    }
}
