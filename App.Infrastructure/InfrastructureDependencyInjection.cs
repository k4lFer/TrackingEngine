using App.Infrastructure.Adapters;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Infrastructure.Core.DataBaseContext.Interceptors;
using App.Infrastructure.Core.Services;
using App.Shared.Objects.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace App.Infrastructure;

public static class InfrastructureDependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        #region DataBase Context
        var connectionString = configuration.GetConnectionString("PostgresSQLConnectionString");
        services.AddSingleton<DomainEventDispatcherInterceptor>();
        services.AddDbContextPool<AppDataBaseContext>((services, options) =>
        {
            var interceptor = services.GetRequiredService<DomainEventDispatcherInterceptor>();

            options.AddInterceptors(interceptor);
            options.UseNpgsql(connectionString, o =>
            {
                o.UseNetTopologySuite();
                o.MapEnum<UserRole>(schemaName: "auth", enumName: "user_role_enum");
                o.MapEnum<UserStatus>(schemaName: "auth", enumName: "user_status_enum");
                o.MapEnum<VerificationType>(schemaName: "user_credential", enumName: "verification_type_enum");
                o.MapEnum<NotificationType>(schemaName: "notifications", enumName: "notification_type_enum");
                o.MapEnum<TripStatus>(schemaName: "tracking", enumName: "trip_status_enum");
                o.MapEnum<TrackingEventType>(schemaName: "tracking", enumName: "tracking_event_type_enum");
                o.MapEnum<EventSeverity>(schemaName: "tracking", enumName: "event_severity_enum");
                o.MapEnum<VehicleState>(schemaName: "fleet", enumName: "vehicle_state_enum");
                o.MapEnum<GeofenceKind>(schemaName: "geofences", enumName: "geofence_kind_enum");
            });
        });
        
        #endregion
        
        services.AddAdapterDependencies(configuration);
        services.AddCoreServices(configuration);
        
        return services;
    }

}