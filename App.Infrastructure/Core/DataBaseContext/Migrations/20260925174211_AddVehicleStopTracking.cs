using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleStopTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "long_stop_notified_at",
                schema: "fleet",
                table: "vehicle_current_state",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "stopped_since",
                schema: "fleet",
                table: "vehicle_current_state",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "long_stop_notified_at",
                schema: "fleet",
                table: "vehicle_current_state");

            migrationBuilder.DropColumn(
                name: "stopped_since",
                schema: "fleet",
                table: "vehicle_current_state");
        }
    }
}
