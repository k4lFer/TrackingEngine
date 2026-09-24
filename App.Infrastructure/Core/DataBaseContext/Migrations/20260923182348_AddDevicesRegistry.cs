using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddDevicesRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicles_device_id",
                schema: "fleet",
                table: "vehicles");

            migrationBuilder.Sql(
                "ALTER TABLE tracking.gps_positions ALTER COLUMN device_id TYPE character varying(64) USING device_id::text;");

            // El registro de dispositivos se puebla ANTES de borrar vehicles.device_id
            // (el seed lee esa columna para conservar el vínculo existente).
            migrationBuilder.CreateTable(
                name: "devices",
                schema: "fleet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identifier = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.id);
                });

            migrationBuilder.Sql(
                """
                INSERT INTO fleet.devices (id, identifier, kind, model, vehicle_id)
                SELECT gen_random_uuid(), device_id::text, 1, NULL, id
                FROM fleet.vehicles
                WHERE device_id IS NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_devices_identifier",
                schema: "fleet",
                table: "devices",
                column: "identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devices_vehicle_id",
                schema: "fleet",
                table: "devices",
                column: "vehicle_id",
                unique: true);

            migrationBuilder.DropColumn(
                name: "device_id",
                schema: "fleet",
                table: "vehicles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "devices",
                schema: "fleet");

            migrationBuilder.AddColumn<int>(
                name: "device_id",
                schema: "fleet",
                table: "vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "device_id",
                schema: "tracking",
                table: "gps_positions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_device_id",
                schema: "fleet",
                table: "vehicles",
                column: "device_id",
                unique: true);
        }
    }
}
