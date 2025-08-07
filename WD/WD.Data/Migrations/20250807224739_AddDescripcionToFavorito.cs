﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WD.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDescripcionToFavorito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "favorito",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "favorito");
        }
    }
}
