using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddGpsPositionsStaging : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gps_positions_staging",
                schema: "tracking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    latitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    longitude = table.Column<double>(type: "double precision", precision: 9, scale: 6, nullable: false),
                    speed_kmh = table.Column<double>(type: "double precision", nullable: true),
                    heading_deg = table.Column<double>(type: "double precision", nullable: true),
                    satellites = table.Column<int>(type: "integer", nullable: true),
                    hdop = table.Column<double>(type: "double precision", nullable: true),
                    ignition = table.Column<bool>(type: "boolean", nullable: true),
                    device_timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed = table.Column<bool>(type: "boolean", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    transport = table.Column<string>(type: "text", nullable: true),
                    decoder = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gps_positions_staging", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_staging_device_id",
                schema: "tracking",
                table: "gps_positions_staging",
                column: "device_id");

            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_staging_processed",
                schema: "tracking",
                table: "gps_positions_staging",
                column: "processed");

            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_staging_received_at",
                schema: "tracking",
                table: "gps_positions_staging",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_staging_vehicle_id_device_timestamp",
                schema: "tracking",
                table: "gps_positions_staging",
                columns: new[] { "vehicle_id", "device_timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gps_positions_staging",
                schema: "tracking");
        }
    }
}
