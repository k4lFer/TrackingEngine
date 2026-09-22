using App.Shared.Validation;
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

        return services;
    }
}