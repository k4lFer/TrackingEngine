using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokensAndUserGateways : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "password",
                schema: "user_credential",
                table: "users",
                type: "varchar",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "user_credential",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    token_hash = table.Column<string>(type: "varchar", maxLength: 255, nullable: false),
                    ip_address = table.Column<string>(type: "varchar", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "varchar", maxLength: 255, nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user_credential",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_gateways",
                schema: "user_credential",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    provider_id = table.Column<string>(type: "varchar", maxLength: 250, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_gateways", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_gateways_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user_credential",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_user_id",
                schema: "user_credential",
                table: "refresh_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_gateways_user_id",
                schema: "user_credential",
                table: "user_gateways",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_tokens",
                schema: "user_credential");

            migrationBuilder.DropTable(
                name: "user_gateways",
                schema: "user_credential");

            migrationBuilder.AlterColumn<string>(
                name: "password",
                schema: "user_credential",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
