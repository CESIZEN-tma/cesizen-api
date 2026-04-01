using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToUserSavedConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "id_user",
                table: "user_saved_configurations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_user_saved_configurations_id_user",
                table: "user_saved_configurations",
                column: "id_user");

            migrationBuilder.AddForeignKey(
                name: "user_saved_configurations_id_user_fk",
                table: "user_saved_configurations",
                column: "id_user",
                principalTable: "users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_saved_configurations_id_user_fk",
                table: "user_saved_configurations");

            migrationBuilder.DropIndex(
                name: "IX_user_saved_configurations_id_user",
                table: "user_saved_configurations");

            migrationBuilder.DropColumn(
                name: "id_user",
                table: "user_saved_configurations");
        }
    }
}
