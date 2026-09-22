using System;
using App.Shared.Objects.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace App.Infrastructure.Core.DataBaseContext.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingFleetRoutesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "geofences");

            migrationBuilder.EnsureSchema(
                name: "tracking");

            migrationBuilder.EnsureSchema(
                name: "materials");

            migrationBuilder.EnsureSchema(
                name: "roads");

            migrationBuilder.EnsureSchema(
                name: "routes");

            migrationBuilder.EnsureSchema(
                name: "fleet");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:auth.user_role_enum", "member,moderator,admin")
                .Annotation("Npgsql:Enum:auth.user_status_enum", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:fleet.vehicle_state_enum", "offline,disponible,en_carga,en_ruta,detenido,en_descarga,fuera_de_servicio")
                .Annotation("Npgsql:Enum:geofences.geofence_kind_enum", "load,unload,wait,restricted,checkpoint,custom")
                .Annotation("Npgsql:Enum:notifications.notification_type_enum", "info,success,warning,error")
                .Annotation("Npgsql:Enum:tracking.event_severity_enum", "info,warning,critical")
                .Annotation("Npgsql:Enum:tracking.tracking_event_type_enum", "geofence_entered,geofence_exited,speed_limit_exceeded,speed_limit_cleared,route_deviation_detected,route_deviation_cleared,long_stop_detected,long_stop_ended,trip_started,trip_completed,trip_cancelled,gps_connection_lost,gps_connection_restored")
                .Annotation("Npgsql:Enum:tracking.trip_status_enum", "active,completed,cancelled,incomplete")
                .Annotation("Npgsql:Enum:user_credential.verification_type_enum", "email_confirmation,password_reset")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .OldAnnotation("Npgsql:Enum:auth.user_role_enum", "member,moderator,admin")
                .OldAnnotation("Npgsql:Enum:auth.user_status_enum", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:notifications.notification_type_enum", "info,success,warning,error")
                .OldAnnotation("Npgsql:Enum:user_credential.verification_type_enum", "email_confirmation,password_reset");

            migrationBuilder.CreateTable(
                name: "geofences",
                schema: "geofences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    kind = table.Column<GeofenceKind>(type: "geofences.geofence_kind_enum", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    geometry = table.Column<Polygon>(type: "geometry (polygon)", nullable: false),
                    max_speed_kmh = table.Column<decimal>(type: "numeric", nullable: true),
                    color = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_geofences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                schema: "materials",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "mine_roads",
                schema: "roads",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    geometry = table.Column<LineString>(type: "geometry (linestring)", nullable: false),
                    max_speed_kmh = table.Column<int>(type: "integer", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mine_roads", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                schema: "fleet",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    last_position = table.Column<Point>(type: "geometry (point)", nullable: true),
                    last_reported_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                schema: "routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    geometry = table.Column<LineString>(type: "geometry (linestring)", nullable: false),
                    tolerance_m = table.Column<int>(type: "integer", nullable: false),
                    max_speed_kmh = table.Column<int>(type: "integer", nullable: true),
                    active = table.Column<bool>(type: "boolean", nullable: false),
                    origin_geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    speed_profile_json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_routes", x => x.id);
                    table.ForeignKey(
                        name: "FK_routes_geofences_destination_geofence_id",
                        column: x => x.destination_geofence_id,
                        principalSchema: "geofences",
                        principalTable: "geofences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_routes_geofences_origin_geofence_id",
                        column: x => x.origin_geofence_id,
                        principalSchema: "geofences",
                        principalTable: "geofences",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "gps_positions",
                schema: "tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<int>(type: "integer", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    geometry = table.Column<Point>(type: "geometry (point)", nullable: false),
                    speed_kmh = table.Column<decimal>(type: "numeric", nullable: true),
                    heading_deg = table.Column<short>(type: "smallint", nullable: true),
                    ignition = table.Column<bool>(type: "boolean", nullable: true),
                    odometer_km = table.Column<decimal>(type: "numeric", nullable: true),
                    hdop = table.Column<decimal>(type: "numeric", nullable: true),
                    satellites = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gps_positions", x => x.id);
                    table.ForeignKey(
                        name: "FK_gps_positions_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "fleet",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_current_state",
                schema: "fleet",
                columns: table => new
                {
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<VehicleState>(type: "fleet.vehicle_state_enum", nullable: false),
                    last_recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_received_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_geom = table.Column<Point>(type: "geometry (point)", nullable: true),
                    current_geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    active_trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    over_speed_since = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    off_route_since = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    off_route_streak = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_current_state", x => x.vehicle_id);
                    table.ForeignKey(
                        name: "FK_vehicle_current_state_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "fleet",
                        principalTable: "vehicles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "trips",
                schema: "tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    origin_geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    destination_geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    material_id = table.Column<Guid>(type: "uuid", nullable: true),
                    load_tonnes = table.Column<decimal>(type: "numeric", nullable: true),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    distance_km = table.Column<decimal>(type: "numeric", nullable: true),
                    duration_s = table.Column<int>(type: "integer", nullable: true),
                    max_speed_kmh = table.Column<decimal>(type: "numeric", nullable: true),
                    deviation_count = table.Column<int>(type: "integer", nullable: false),
                    speed_alert_count = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<TripStatus>(type: "tracking.trip_status_enum", nullable: false),
                    track = table.Column<LineString>(type: "geometry (linestring)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trips", x => x.id);
                    table.ForeignKey(
                        name: "FK_trips_geofences_destination_geofence_id",
                        column: x => x.destination_geofence_id,
                        principalSchema: "geofences",
                        principalTable: "geofences",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trips_geofences_origin_geofence_id",
                        column: x => x.origin_geofence_id,
                        principalSchema: "geofences",
                        principalTable: "geofences",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trips_materials_material_id",
                        column: x => x.material_id,
                        principalSchema: "materials",
                        principalTable: "materials",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trips_routes_route_id",
                        column: x => x.route_id,
                        principalSchema: "routes",
                        principalTable: "routes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_trips_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "fleet",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tracking_events",
                schema: "tracking",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<TrackingEventType>(type: "tracking.tracking_event_type_enum", nullable: false),
                    severity = table.Column<EventSeverity>(type: "tracking.event_severity_enum", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    geofence_id = table.Column<Guid>(type: "uuid", nullable: true),
                    position = table.Column<Point>(type: "geometry (point)", nullable: true),
                    payload_json = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tracking_events", x => x.id);
                    table.ForeignKey(
                        name: "FK_tracking_events_geofences_geofence_id",
                        column: x => x.geofence_id,
                        principalSchema: "geofences",
                        principalTable: "geofences",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracking_events_trips_trip_id",
                        column: x => x.trip_id,
                        principalSchema: "tracking",
                        principalTable: "trips",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tracking_events_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalSchema: "fleet",
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_geofences_active",
                schema: "geofences",
                table: "geofences",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_geofences_code",
                schema: "geofences",
                table: "geofences",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_recorded_at",
                schema: "tracking",
                table: "gps_positions",
                column: "recorded_at");

            migrationBuilder.CreateIndex(
                name: "IX_gps_positions_vehicle_id",
                schema: "tracking",
                table: "gps_positions",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_materials_code",
                schema: "materials",
                table: "materials",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_mine_roads_active",
                schema: "roads",
                table: "mine_roads",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_mine_roads_code",
                schema: "roads",
                table: "mine_roads",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routes_active",
                schema: "routes",
                table: "routes",
                column: "active");

            migrationBuilder.CreateIndex(
                name: "IX_routes_code",
                schema: "routes",
                table: "routes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_routes_destination_geofence_id",
                schema: "routes",
                table: "routes",
                column: "destination_geofence_id");

            migrationBuilder.CreateIndex(
                name: "IX_routes_origin_geofence_id",
                schema: "routes",
                table: "routes",
                column: "origin_geofence_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_events_geofence_id",
                schema: "tracking",
                table: "tracking_events",
                column: "geofence_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_events_occurred_at",
                schema: "tracking",
                table: "tracking_events",
                column: "occurred_at");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_events_trip_id",
                schema: "tracking",
                table: "tracking_events",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "IX_tracking_events_vehicle_id",
                schema: "tracking",
                table: "tracking_events",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_trips_destination_geofence_id",
                schema: "tracking",
                table: "trips",
                column: "destination_geofence_id");

            migrationBuilder.CreateIndex(
                name: "IX_trips_material_id",
                schema: "tracking",
                table: "trips",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_trips_origin_geofence_id",
                schema: "tracking",
                table: "trips",
                column: "origin_geofence_id");

            migrationBuilder.CreateIndex(
                name: "IX_trips_route_id",
                schema: "tracking",
                table: "trips",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_trips_status",
                schema: "tracking",
                table: "trips",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_trips_vehicle_id",
                schema: "tracking",
                table: "trips",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_code",
                schema: "fleet",
                table: "vehicles",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gps_positions",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "mine_roads",
                schema: "roads");

            migrationBuilder.DropTable(
                name: "tracking_events",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "vehicle_current_state",
                schema: "fleet");

            migrationBuilder.DropTable(
                name: "trips",
                schema: "tracking");

            migrationBuilder.DropTable(
                name: "materials",
                schema: "materials");

            migrationBuilder.DropTable(
                name: "routes",
                schema: "routes");

            migrationBuilder.DropTable(
                name: "vehicles",
                schema: "fleet");

            migrationBuilder.DropTable(
                name: "geofences",
                schema: "geofences");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:auth.user_role_enum", "member,moderator,admin")
                .Annotation("Npgsql:Enum:auth.user_status_enum", "active,inactive,suspended")
                .Annotation("Npgsql:Enum:notifications.notification_type_enum", "info,success,warning,error")
                .Annotation("Npgsql:Enum:user_credential.verification_type_enum", "email_confirmation,password_reset")
                .OldAnnotation("Npgsql:Enum:auth.user_role_enum", "member,moderator,admin")
                .OldAnnotation("Npgsql:Enum:auth.user_status_enum", "active,inactive,suspended")
                .OldAnnotation("Npgsql:Enum:fleet.vehicle_state_enum", "offline,disponible,en_carga,en_ruta,detenido,en_descarga,fuera_de_servicio")
                .OldAnnotation("Npgsql:Enum:geofences.geofence_kind_enum", "load,unload,wait,restricted,checkpoint,custom")
                .OldAnnotation("Npgsql:Enum:notifications.notification_type_enum", "info,success,warning,error")
                .OldAnnotation("Npgsql:Enum:tracking.event_severity_enum", "info,warning,critical")
                .OldAnnotation("Npgsql:Enum:tracking.tracking_event_type_enum", "geofence_entered,geofence_exited,speed_limit_exceeded,speed_limit_cleared,route_deviation_detected,route_deviation_cleared,long_stop_detected,long_stop_ended,trip_started,trip_completed,trip_cancelled,gps_connection_lost,gps_connection_restored")
                .OldAnnotation("Npgsql:Enum:tracking.trip_status_enum", "active,completed,cancelled,incomplete")
                .OldAnnotation("Npgsql:Enum:user_credential.verification_type_enum", "email_confirmation,password_reset")
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");
        }
    }
}
