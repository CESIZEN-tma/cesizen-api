using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_password_reset_tokens_passwords_infos_PasswordsInfoId",
                table: "password_reset_tokens");

            migrationBuilder.DropIndex(
                name: "IX_password_reset_tokens_PasswordsInfoId",
                table: "password_reset_tokens");

            migrationBuilder.DropColumn(
                name: "PasswordsInfoId",
                table: "password_reset_tokens");

            migrationBuilder.AddColumn<Guid>(
                name: "IdPasswordsInfos",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IdPasswordsInfosNavigationId",
                table: "users",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_IdPasswordsInfosNavigationId",
                table: "users",
                column: "IdPasswordsInfosNavigationId");

            migrationBuilder.AddForeignKey(
                name: "FK_users_passwords_infos_IdPasswordsInfosNavigationId",
                table: "users",
                column: "IdPasswordsInfosNavigationId",
                principalTable: "passwords_infos",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_users_passwords_infos_IdPasswordsInfosNavigationId",
                table: "users");

            migrationBuilder.DropIndex(
                name: "IX_users_IdPasswordsInfosNavigationId",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IdPasswordsInfos",
                table: "users");

            migrationBuilder.DropColumn(
                name: "IdPasswordsInfosNavigationId",
                table: "users");

            migrationBuilder.AddColumn<Guid>(
                name: "PasswordsInfoId",
                table: "password_reset_tokens",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_PasswordsInfoId",
                table: "password_reset_tokens",
                column: "PasswordsInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_password_reset_tokens_passwords_infos_PasswordsInfoId",
                table: "password_reset_tokens",
                column: "PasswordsInfoId",
                principalTable: "passwords_infos",
                principalColumn: "id");
        }
    }
}
