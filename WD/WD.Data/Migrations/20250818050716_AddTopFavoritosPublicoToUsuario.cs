using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTopFavoritosPublicoToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TopFavoritosPublico",
                table: "usuario",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TopFavoritosPublico",
                table: "usuario");
        }
    }
}
