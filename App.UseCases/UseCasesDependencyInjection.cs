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

        // El filtro DEBE ser singleton: guarda el último punto de cada vehículo, y
        // el gateway TCP crea un scope por trama, así que con lifetime Scoped se
        // reiniciaría en cada envío y no filtraría nada.
        services.AddSingleton<IVehicleRealtimeGate, App.UseCases.Gps.VehicleRealtimeGate>();

        services.AddScoped<Gps.GpsIngestionService>();

        return services;
    }
}