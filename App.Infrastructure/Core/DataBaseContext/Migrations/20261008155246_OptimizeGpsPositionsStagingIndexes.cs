using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeGpsPositionsStagingIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_gps_positions_staging_device_id",
                schema: "tracking",
                table: "gps_positions_staging");

            migrationBuilder.DropIndex(
                name: "IX_gps_positions_staging_processed",
                schema: "tracking",
                table: "gps_positions_staging");

            migrationBuilder.DropIndex(
                name: "IX_gps_positions_staging_received_at",
                schema: "tracking",
                table: "gps_positions_staging");

            migrationBuilder.DropIndex(
                name: "IX_gps_positions_staging_vehicle_id_device_timestamp",
                schema: "tracking",
                table: "gps_positions_staging");

            migrationBuilder.CreateIndex(
                name: "ix_gps_positions_staging_received_at_unprocessed",
                schema: "tracking",
                table: "gps_positions_staging",
                column: "received_at",
                filter: "\"processed\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_gps_positions_staging_received_at_unprocessed",
                schema: "tracking",
                table: "gps_positions_staging");

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
    }
}
