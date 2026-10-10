using App.Interfaces.Ports.Tracking;
using App.Shared.Common.Validation;
using Cortex.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using App.UseCases.Tracking.Services;
using App.UseCases.Gps.Batching;
using App.UseCases.Gps.Caching;
using App.UseCases.Gps;
using App.UseCases.Gps.Decoders;

namespace App.UseCases;

public static class UseCasesDependencyInjection
{
    public static IServiceCollection AddUseCasesDi(this IServiceCollection services)
    {
        services.AddCortexMediator(
            [typeof(UseCasesDependencyInjection)]
        );

        services.Scan(scan => scan
            .FromAssemblies(typeof(UseCasesDependencyInjection).Assembly)
            .AddClasses(c => c.AssignableTo(typeof(IInputValidator<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        // ── Ingesta GPS genérica: un decodificador por protocolo. ─────────────
        services.AddSingleton<IGpsDeviceDecoder, OsmAndQueryDecoder>();
        services.AddSingleton<IGpsDeviceDecoder, Tk103FrameDecoder>();

        services.AddSingleton<IVehicleRealtimeGate, VehicleRealtimeGate>();

        services.AddSingleton<IGpsIngestCache, GpsIngestCache>();
        services.AddHostedService<GpsBatchWorker>();
        // Preheat de una sola pasada; debe arrancar ANTES que GpsTcpGatewayHostedService
        // para que el pico de reconexión no caiga en cache-miss masivo.
        services.AddHostedService<App.UseCases.Gps.Warming.GpsVehicleCacheWarmer>();

        services.AddScoped<IPositionProcessor, PositionProcessor>();

        services.AddScoped<GpsIngestionService>();

        return services;
    }
}