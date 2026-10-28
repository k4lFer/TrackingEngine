using App.Interfaces.Ports.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace App.Infrastructure.Adapters.Messaging.Redis;

public static class RedisMessagingDependencyInjection
{
    public static IServiceCollection AddRedisMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisGpsStreamOptions>(configuration.GetSection(RedisGpsStreamOptions.SectionName));

        var options = configuration.GetSection(RedisGpsStreamOptions.SectionName).Get<RedisGpsStreamOptions>() ?? new RedisGpsStreamOptions();
        var config = string.IsNullOrWhiteSpace(options.Configuration)
            ? configuration.GetConnectionString("Redis") ?? "localhost:6379"
            : options.Configuration;

        if (string.IsNullOrWhiteSpace(options.Configuration))
        {
            options.Configuration = config;
        }

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(config));
        services.AddSingleton<IGpsEventPublisher, RedisGpsStreamPublisher>();

        return services;
    }
}
