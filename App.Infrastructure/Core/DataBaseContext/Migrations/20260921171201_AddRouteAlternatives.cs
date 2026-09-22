using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddRouteAlternatives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "alternative_rank",
                schema: "routes",
                table: "routes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "route_group_id",
                schema: "routes",
                table: "routes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_routes_route_group_id",
                schema: "routes",
                table: "routes",
                column: "route_group_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_routes_route_group_id",
                schema: "routes",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "alternative_rank",
                schema: "routes",
                table: "routes");

            migrationBuilder.DropColumn(
                name: "route_group_id",
                schema: "routes",
                table: "routes");
        }
    }
}
