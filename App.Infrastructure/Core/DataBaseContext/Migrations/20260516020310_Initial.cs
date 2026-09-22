using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "user_credential");

            migrationBuilder.CreateTable(
                name: "domain_events",
                schema: "audit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "varchar", maxLength: 500, nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_domain_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "user_credential",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "varchar", maxLength: 150, nullable: false),
                    username = table.Column<string>(type: "varchar", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    is_email_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                schema: "user_credential",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar", maxLength: 100, nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persons", x => x.id);
                    table.ForeignKey(
                        name: "FK_persons_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "user_credential",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_domain_events_occurred_at_utc",
                schema: "audit",
                table: "domain_events",
                column: "occurred_at_utc");

            migrationBuilder.CreateIndex(
                name: "ix_domain_events_type",
                schema: "audit",
                table: "domain_events",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "IX_persons_user_id",
                schema: "user_credential",
                table: "persons",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "domain_events",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "persons",
                schema: "user_credential");

            migrationBuilder.DropTable(
                name: "users",
                schema: "user_credential");
        }
    }
}
