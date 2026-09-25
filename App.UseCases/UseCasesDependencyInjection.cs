using App.Interfaces.Ports.Tracking;
using App.Shared.Common.Validation;
using Cortex.Mediator.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddSingleton<IGpsDeviceDecoder, App.UseCases.Gps.Decoders.OsmAndQueryDecoder>();
        services.AddSingleton<IGpsDeviceDecoder, App.UseCases.Gps.Decoders.Tk103FrameDecoder>();
        services.AddScoped<Gps.GpsIngestionService>();

        return services;
    }
}