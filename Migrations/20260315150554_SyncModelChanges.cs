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
            // These objects may not exist if the schema was bootstrapped from init.sql
            // which already had the refactored FK naming. Safe to skip if absent.
            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_password_reset_tokens_passwords_infos_PasswordsInfoId'
                        AND table_name = 'password_reset_tokens'
                    ) THEN
                        ALTER TABLE password_reset_tokens DROP CONSTRAINT ""FK_password_reset_tokens_passwords_infos_PasswordsInfoId"";
                    END IF;
                END $$;");

            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ""IX_password_reset_tokens_PasswordsInfoId"";");

            migrationBuilder.Sql(@"
                ALTER TABLE password_reset_tokens DROP COLUMN IF EXISTS ""PasswordsInfoId"";");

            migrationBuilder.Sql(@"
                ALTER TABLE users ADD COLUMN IF NOT EXISTS ""IdPasswordsInfos"" uuid;
                ALTER TABLE users ADD COLUMN IF NOT EXISTS ""IdPasswordsInfosNavigationId"" uuid;");

            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ""IX_users_IdPasswordsInfosNavigationId""
                ON users (""IdPasswordsInfosNavigationId"");");

            migrationBuilder.Sql(@"
                DO $$ BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.table_constraints
                        WHERE constraint_name = 'FK_users_passwords_infos_IdPasswordsInfosNavigationId'
                        AND table_name = 'users'
                    ) THEN
                        ALTER TABLE users ADD CONSTRAINT ""FK_users_passwords_infos_IdPasswordsInfosNavigationId""
                        FOREIGN KEY (""IdPasswordsInfosNavigationId"") REFERENCES passwords_infos (id);
                    END IF;
                END $$;");
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
