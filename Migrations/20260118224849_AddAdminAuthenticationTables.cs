using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminAuthenticationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_email_confirmation_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    consumed = table.Column<bool>(type: "boolean", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_administrators = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("admin_email_confirmation_tokens_pk", x => x.id);
                    table.ForeignKey(
                        name: "admin_email_confirmation_tokens_id_administrators_fk",
                        column: x => x.id_administrators,
                        principalTable: "administrators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "admin_password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    consumed = table.Column<bool>(type: "boolean", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_administrators = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("admin_password_reset_tokens_pk", x => x.id);
                    table.ForeignKey(
                        name: "admin_password_reset_tokens_id_administrators_fk",
                        column: x => x.id_administrators,
                        principalTable: "administrators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "admin_session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    consumed = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_administrators = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("admin_session_pk", x => x.id);
                    table.ForeignKey(
                        name: "admin_session_id_administrators_fk",
                        column: x => x.id_administrators,
                        principalTable: "administrators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_email_confirmation_tokens_id_administrators",
                table: "admin_email_confirmation_tokens",
                column: "id_administrators");

            migrationBuilder.CreateIndex(
                name: "admin_token_idx",
                table: "admin_password_reset_tokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_admin_password_reset_tokens_id_administrators",
                table: "admin_password_reset_tokens",
                column: "id_administrators");

            migrationBuilder.CreateIndex(
                name: "IX_admin_session_id_administrators",
                table: "admin_session",
                column: "id_administrators");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_email_confirmation_tokens");

            migrationBuilder.DropTable(
                name: "admin_password_reset_tokens");

            migrationBuilder.DropTable(
                name: "admin_session");
        }
    }
}
