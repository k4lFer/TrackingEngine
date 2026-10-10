using App.Interfaces.Ports.Vehicles;
using App.UseCases.Gps.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.UseCases.Gps.Warming;

public sealed class GpsVehicleCacheWarmer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGpsIngestCache _cache;
    private readonly ILogger<GpsVehicleCacheWarmer> _logger;

    public GpsVehicleCacheWarmer(
        IServiceScopeFactory scopeFactory,
        IGpsIngestCache cache,
        ILogger<GpsVehicleCacheWarmer> logger)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var vehicles = scope.ServiceProvider.GetRequiredService<IVehicleRepository>();
            var pairs = await vehicles.GetAllDeviceVehiclePairsAsync(cancellationToken);

            var warmed = 0;
            foreach (var (identifier, vehicleId) in pairs)
            {
                if (!_cache.TryGetVehicleId(identifier, out _))
                {
                    _cache.SetVehicleId(identifier, vehicleId);
                }
                warmed++;
            }

            _logger.LogInformation("[gps] cache device→vehicle precalentado con {Count} dispositivos", warmed);
        }
        catch (OperationCanceledException)
        {
            // Shutdown en marcha: no se pudo precalentar, se resolverá bajo demanda.
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[gps] fallo al precalentar cache device→vehicle; se resolverá bajo demanda");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}