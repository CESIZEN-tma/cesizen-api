using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPasswordResetTokenFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "information_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("information_tags_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "navigation_menu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    url = table.Column<string>(type: "text", nullable: false),
                    currently_editing = table.Column<bool>(type: "boolean", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("navigation_menu_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "passwords_infos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    last_login = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    last_reset = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("passwords_infos_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "quizz",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nom = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("quizz_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_saved_configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    inhalation = table.Column<int>(type: "integer", nullable: false),
                    retention1 = table.Column<int>(type: "integer", nullable: false),
                    exhalation = table.Column<int>(type: "integer", nullable: false),
                    retention2 = table.Column<int>(type: "integer", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    objective = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    guidance_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_saved_configurations_pk", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "administrators",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    last_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    member_since = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    account_activated = table.Column<bool>(type: "boolean", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_navigation_menu = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("administrators_pk", x => x.id);
                    table.ForeignKey(
                        name: "administrators_id_navigation_menu_fk",
                        column: x => x.id_navigation_menu,
                        principalTable: "navigation_menu",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "password_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    changed_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_passwords_infos = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("password_history_pk", x => x.id);
                    table.ForeignKey(
                        name: "password_history_id_passwords_infos_fk",
                        column: x => x.id_passwords_infos,
                        principalTable: "passwords_infos",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "questions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_quizz = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("questions_pk", x => x.id);
                    table.ForeignKey(
                        name: "questions_id_quizz_fk",
                        column: x => x.id_quizz,
                        principalTable: "quizz",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    last_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    member_since = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    thumbnail_url = table.Column<string>(type: "text", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    account_activated = table.Column<bool>(type: "boolean", nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_user_saved_configurations = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pk", x => x.id);
                    table.ForeignKey(
                        name: "users_id_user_saved_configurations_fk",
                        column: x => x.id_user_saved_configurations,
                        principalTable: "user_saved_configurations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "admin_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    action_code = table.Column<string>(type: "text", nullable: false),
                    entity_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    targeted_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_administrator = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("admin_logs_pk", x => x.id);
                    table.ForeignKey(
                        name: "admin_logs_id_administrator_fk",
                        column: x => x.id_administrator,
                        principalTable: "administrators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "configurations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    inhalation = table.Column<int>(type: "integer", nullable: false),
                    retention1 = table.Column<int>(type: "integer", nullable: false),
                    exhalation = table.Column<int>(type: "integer", nullable: false),
                    retention2 = table.Column<int>(type: "integer", nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    objective = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    guidance_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_administrators = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("configurations_pk", x => x.id);
                    table.ForeignKey(
                        name: "configurations_id_administrators_fk",
                        column: x => x.id_administrators,
                        principalTable: "administrators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "information_pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    currently_editing = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_administrators = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("information_pages_pk", x => x.id);
                    table.ForeignKey(
                        name: "information_pages_id_administrators_fk",
                        column: x => x.id_administrators,
                        principalTable: "administrators",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "responses_options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false),
                    targeted_field = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    operation = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_questions = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("responses_options_pk", x => x.id);
                    table.ForeignKey(
                        name: "responses_options_id_questions_fk",
                        column: x => x.id_questions,
                        principalTable: "questions",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "email_confirmation_tokens",
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
                    id_users = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("email_confirmation_tokens_pk", x => x.id);
                    table.ForeignKey(
                        name: "email_confirmation_tokens_id_users_fk",
                        column: x => x.id_users,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
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
                    id_users = table.Column<Guid>(type: "uuid", nullable: false),
                    PasswordsInfoId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("password_reset_tokens_pk", x => x.id);
                    table.ForeignKey(
                        name: "FK_password_reset_tokens_passwords_infos_PasswordsInfoId",
                        column: x => x.PasswordsInfoId,
                        principalTable: "passwords_infos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "password_reset_tokens_id_users_fk",
                        column: x => x.id_users,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(type: "text", nullable: false),
                    consumed = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    id_users = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("session_pk", x => x.id);
                    table.ForeignKey(
                        name: "session_id_users_fk",
                        column: x => x.id_users,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "bookmark",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_configurations = table.Column<Guid>(type: "uuid", nullable: false),
                    creation_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    update_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    deletion_time = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("bookmark_pk", x => new { x.id, x.id_configurations });
                    table.ForeignKey(
                        name: "bookmark_id_configurations_fk",
                        column: x => x.id_configurations,
                        principalTable: "configurations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "bookmark_id_fk",
                        column: x => x.id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tagged",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_information_tags = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tagged_pk", x => new { x.id, x.id_information_tags });
                    table.ForeignKey(
                        name: "tagged_id_fk",
                        column: x => x.id,
                        principalTable: "information_pages",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "tagged_id_information_tags_fk",
                        column: x => x.id_information_tags,
                        principalTable: "information_tags",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "auth",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_session = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("auth_pk", x => new { x.id, x.id_session });
                    table.ForeignKey(
                        name: "auth_id_fk",
                        column: x => x.id,
                        principalTable: "administrators",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "auth_id_session_fk",
                        column: x => x.id_session,
                        principalTable: "session",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_logs_id_administrator",
                table: "admin_logs",
                column: "id_administrator");

            migrationBuilder.CreateIndex(
                name: "email_admin_idx",
                table: "administrators",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "email_admin_unq",
                table: "administrators",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_administrators_id_navigation_menu",
                table: "administrators",
                column: "id_navigation_menu");

            migrationBuilder.CreateIndex(
                name: "IX_auth_id_session",
                table: "auth",
                column: "id_session");

            migrationBuilder.CreateIndex(
                name: "IX_bookmark_id_configurations",
                table: "bookmark",
                column: "id_configurations");

            migrationBuilder.CreateIndex(
                name: "IX_configurations_id_administrators",
                table: "configurations",
                column: "id_administrators");

            migrationBuilder.CreateIndex(
                name: "IX_email_confirmation_tokens_id_users",
                table: "email_confirmation_tokens",
                column: "id_users");

            migrationBuilder.CreateIndex(
                name: "IX_information_pages_id_administrators",
                table: "information_pages",
                column: "id_administrators");

            migrationBuilder.CreateIndex(
                name: "IX_password_history_id_passwords_infos",
                table: "password_history",
                column: "id_passwords_infos");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_id_users",
                table: "password_reset_tokens",
                column: "id_users");

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_PasswordsInfoId",
                table: "password_reset_tokens",
                column: "PasswordsInfoId");

            migrationBuilder.CreateIndex(
                name: "token_idx",
                table: "password_reset_tokens",
                column: "token");

            migrationBuilder.CreateIndex(
                name: "IX_questions_id_quizz",
                table: "questions",
                column: "id_quizz");

            migrationBuilder.CreateIndex(
                name: "IX_responses_options_id_questions",
                table: "responses_options",
                column: "id_questions");

            migrationBuilder.CreateIndex(
                name: "IX_session_id_users",
                table: "session",
                column: "id_users");

            migrationBuilder.CreateIndex(
                name: "IX_tagged_id_information_tags",
                table: "tagged",
                column: "id_information_tags");

            migrationBuilder.CreateIndex(
                name: "email_idx",
                table: "users",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "email_unq",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_id_user_saved_configurations",
                table: "users",
                column: "id_user_saved_configurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_logs");

            migrationBuilder.DropTable(
                name: "auth");

            migrationBuilder.DropTable(
                name: "bookmark");

            migrationBuilder.DropTable(
                name: "email_confirmation_tokens");

            migrationBuilder.DropTable(
                name: "password_history");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "responses_options");

            migrationBuilder.DropTable(
                name: "tagged");

            migrationBuilder.DropTable(
                name: "session");

            migrationBuilder.DropTable(
                name: "configurations");

            migrationBuilder.DropTable(
                name: "passwords_infos");

            migrationBuilder.DropTable(
                name: "questions");

            migrationBuilder.DropTable(
                name: "information_pages");

            migrationBuilder.DropTable(
                name: "information_tags");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "quizz");

            migrationBuilder.DropTable(
                name: "administrators");

            migrationBuilder.DropTable(
                name: "user_saved_configurations");

            migrationBuilder.DropTable(
                name: "navigation_menu");
        }
    }
}
