using App.Shared.Common.Enums;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Core.DataBaseContext.Configurations.Global;

public static class EnumConfigurations
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresEnum<UserRole>(schema: "auth", name: "user_role_enum");
        modelBuilder.HasPostgresEnum<UserStatus>(schema: "auth", name: "user_status_enum");
        modelBuilder.HasPostgresEnum<VerificationType>(schema: "user_credential", name: "verification_type_enum");
        modelBuilder.HasPostgresEnum<NotificationType>(schema: "notifications", name: "notification_type_enum");

        modelBuilder.HasPostgresEnum<TripStatus>(schema: "tracking", name: "trip_status_enum");
        modelBuilder.HasPostgresEnum<TrackingEventType>(schema: "tracking", name: "tracking_event_type_enum");
        modelBuilder.HasPostgresEnum<EventSeverity>(schema: "tracking", name: "event_severity_enum");
        modelBuilder.HasPostgresEnum<VehicleState>(schema: "fleet", name: "vehicle_state_enum");
        modelBuilder.HasPostgresEnum<GeofenceKind>(schema: "geofences", name: "geofence_kind_enum");
    }
}