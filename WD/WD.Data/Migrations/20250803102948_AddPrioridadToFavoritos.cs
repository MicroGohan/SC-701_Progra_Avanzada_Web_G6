using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPrioridadToFavoritos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "favorito",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "favorito");
        }
    }
}
