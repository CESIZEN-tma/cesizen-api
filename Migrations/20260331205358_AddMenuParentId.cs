using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddMenuParentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "navigation_menu",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "parent_id",
                table: "navigation_menu",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_navigation_menu_parent_id",
                table: "navigation_menu",
                column: "parent_id");

            migrationBuilder.AddForeignKey(
                name: "FK_navigation_menu_navigation_menu_parent_id",
                table: "navigation_menu",
                column: "parent_id",
                principalTable: "navigation_menu",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_navigation_menu_navigation_menu_parent_id",
                table: "navigation_menu");

            migrationBuilder.DropIndex(
                name: "IX_navigation_menu_parent_id",
                table: "navigation_menu");

            migrationBuilder.DropColumn(
                name: "parent_id",
                table: "navigation_menu");

            migrationBuilder.AlterColumn<string>(
                name: "url",
                table: "navigation_menu",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
