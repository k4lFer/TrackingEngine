using System.Text.Json;
using App.Interfaces.Ports.Messaging;
using App.Objects.Gps.DTOs.Output.Response;
using App.Objects.Gps.Events;
using App.UseCases.Gps;
using StackExchange.Redis;

namespace WebApi.Services;

public sealed class GpsEventConsumerHostedService : BackgroundService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GpsEventConsumerHostedService> _logger;
    private readonly string _streamKey;
    private readonly string _consumerGroup;
    private readonly string _consumerName;

    public GpsEventConsumerHostedService(
        IConfiguration configuration,
        IConnectionMultiplexer multiplexer,
        IServiceScopeFactory scopeFactory,
        ILogger<GpsEventConsumerHostedService> logger)
    {
        _multiplexer = multiplexer;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _streamKey = configuration["Redis:StreamKey"] ?? "gps:positions:stream";
        _consumerGroup = configuration["Redis:ConsumerGroup"] ?? "app-consumers";
        _consumerName = $"app-{Environment.MachineName}-{Guid.NewGuid():N}";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var db = _multiplexer.GetDatabase();

        try
        {
            await db.StreamCreateConsumerGroupAsync(_streamKey, _consumerGroup, "0-0", createStream: true);
            _logger.LogInformation("Consumer group {Group} creado/asegurado en stream {Stream}", _consumerGroup, _streamKey);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Ya existe
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo crear el consumer group {Group} en {Stream}", _consumerGroup, _streamKey);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var entries = await db.StreamReadGroupAsync(_streamKey, _consumerGroup, _consumerName, ">", count: 100, noAck: false);

                if (entries.Length == 0)
                {
                    await Task.Delay(100, stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    try
                    {
                        var payload = entry.Values.FirstOrDefault(v => v.Name == "payload").Value;
                        if (payload.IsNullOrEmpty) 
                        {
                            await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
                            continue;
                        }

                        var evt = JsonSerializer.Deserialize<GpsPositionReceived>(payload.ToString(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (evt is null)
                        {
                            await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
                            continue;
                        }

                        await ProcessEventAsync(evt, stoppingToken);
                        await db.StreamAcknowledgeAsync(_streamKey, _consumerGroup, entry.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error procesando entrada {Id} del stream GPS", entry.Id);
                        // No ACK para reintento
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el consumer de GPS");
                await Task.Delay(1000, stoppingToken);
            }
        }
    }

    private async Task ProcessEventAsync(GpsPositionReceived evt, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var gps = scope.ServiceProvider.GetRequiredService<GpsIngestionService>();

        // Por ahora, delegamos al servicio existente. 
        // El gateway seguirá delegando directo hasta refactorizarlo para publicar eventos puros.
        // Aquí mantenemos compatibilidad: si llega con RawFrame, lo procesamos.
        if (!string.IsNullOrWhiteSpace(evt.RawFrame))
        {
            var bytes = System.Text.Encoding.ASCII.GetBytes(evt.RawFrame);
            await gps.HandleAsync(evt.Source ?? "tcp", bytes, cancellationToken);
            return;
        }

        // Si no hay RawFrame, no podemos reprocesar con decodificadores actuales
        _logger.LogDebug("Evento GPS sin RawFrame recibido. Ignorado por consumidor actual.");
    }
}
