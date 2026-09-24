using App.Infrastructure.Adapters.Devices;
using App.Infrastructure.Adapters.Geofences;
using App.Infrastructure.Adapters.Materials;
using App.Infrastructure.Adapters.Roads;
using App.Infrastructure.Adapters.Routes;
using App.Infrastructure.Adapters.Tracking;
using App.Infrastructure.Adapters.User;
using App.Infrastructure.Adapters.Vehicles;
using App.Interfaces.Ports;
using App.Interfaces.Ports.Devices;
using App.Interfaces.Ports.Geofences;
using App.Interfaces.Ports.Materials;
using App.Interfaces.Ports.Roads;
using App.Interfaces.Ports.Routes;
using App.Interfaces.Ports.Tracking;
using App.Interfaces.Ports.User;
using App.Interfaces.Ports.Vehicles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure.Adapters;

public static class AdapterDependencyInjection 
{
    public static IServiceCollection AddAdapterDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserQueryRepository, UserQueryRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRefreshTokenQueryRepository, RefreshTokenQueryRepository>();
        services.AddScoped<IVerificationRepository, VerificationRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehicleQueryRepository, VehicleQueryRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IGeofenceRepository, GeofenceRepository>();
        services.AddScoped<IGeofenceQueryRepository, GeofenceQueryRepository>();
        services.AddScoped<IMaterialRepository, MaterialRepository>();
        services.AddScoped<IMaterialQueryRepository, MaterialRepository>();
        services.AddScoped<IRoadRepository, RoadRepository>();
        services.AddScoped<IRoadQueryRepository, RoadQueryRepository>();
        services.AddScoped<IRouteRepository, RouteRepository>();
        services.AddScoped<IRouteGeometryRepository, RouteRepository>();
        services.AddScoped<IRouteQueryRepository, RouteQueryRepository>();
        services.AddScoped<ITrackingReadRepository, TrackingRepository>();
        services.AddScoped<ITrackingWriteRepository, TrackingRepository>();

        var valhalla = configuration.GetSection("Routing:Valhalla").Get<ValhallaOptions>() ?? new ValhallaOptions();
        services.AddSingleton(valhalla);
        services.AddHttpClient<IRoutePlanner, ValhallaRoutePlanner>((sp, client) =>
        {
            client.BaseAddress = new Uri(valhalla.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(valhalla.TimeoutSeconds);
        });

        return services;
    }
}