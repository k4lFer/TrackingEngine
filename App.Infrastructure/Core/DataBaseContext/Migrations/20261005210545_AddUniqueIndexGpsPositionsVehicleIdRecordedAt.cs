using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexGpsPositionsVehicleIdRecordedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_vehicle_id_recorded_at",
                schema: "tracking",
                table: "gps_positions",
                columns: new[] { "vehicle_id", "recorded_at" },
                unique: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_gps_positions_vehicle_id_recorded_at",
                schema: "tracking",
                table: "gps_positions");
        }
    }
}
