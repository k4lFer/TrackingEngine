using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleDeviceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "device_id",
                schema: "fleet",
                table: "vehicles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_device_id",
                schema: "fleet",
                table: "vehicles",
                column: "device_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_vehicles_device_id",
                schema: "fleet",
                table: "vehicles");

            migrationBuilder.DropColumn(
                name: "device_id",
                schema: "fleet",
                table: "vehicles");
        }
    }
}
