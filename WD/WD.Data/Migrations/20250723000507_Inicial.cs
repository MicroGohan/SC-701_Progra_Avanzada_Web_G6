using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WD.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fecha_registro = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__usuario__4E3E04ADE1EA58A0", x => x.id_usuario);
                });

            migrationBuilder.CreateTable(
                name: "favorito",
                columns: table => new
                {
                    id_favorito = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    ciudad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    pais = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fecha_agregado = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__favorito__78F875AE89CD8F09", x => x.id_favorito);
                    table.ForeignKey(
                        name: "FK__favorito__id_usu__3C69FB99",
                        column: x => x.id_usuario,
                        principalTable: "usuario",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_favorito_id_usuario",
                table: "favorito",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "UQ__usuario__AB6E6164252565AF",
                table: "usuario",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "favorito");

            migrationBuilder.DropTable(
                name: "usuario");
        }
    }
}
