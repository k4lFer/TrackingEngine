using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "email_verifications",
                schema: "user_credential",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "varchar", maxLength: 150, nullable: false),
                    token_hash = table.Column<string>(type: "varchar", maxLength: 128, nullable: false),
                    code_hash = table.Column<string>(type: "varchar", maxLength: 128, nullable: false),
                    type = table.Column<string>(type: "varchar", maxLength: 30, nullable: false),
                    link_expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    code_expires_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    used_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    failed_attempts = table.Column<int>(type: "integer", nullable: false),
                    is_valid = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_verifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_email_verifications_code_hash",
                schema: "user_credential",
                table: "email_verifications",
                column: "code_hash");

            migrationBuilder.CreateIndex(
                name: "IX_email_verifications_token_hash",
                schema: "user_credential",
                table: "email_verifications",
                column: "token_hash");

            migrationBuilder.CreateIndex(
                name: "IX_email_verifications_user_id",
                schema: "user_credential",
                table: "email_verifications",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "email_verifications",
                schema: "user_credential");
        }
    }
}
