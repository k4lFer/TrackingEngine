using System.Text.Json;
using App.Interfaces.Ports.Messaging;
using App.Objects.Gps.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace App.Infrastructure.Adapters.Messaging.Redis;

public sealed class RedisGpsStreamPublisher : IGpsEventPublisher
{
    private readonly IConnectionMultiplexer _multiplexer;
    private readonly IDatabase _database;
    private readonly RedisGpsStreamOptions _options;
    private readonly ILogger<RedisGpsStreamPublisher> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public RedisGpsStreamPublisher(
        IConnectionMultiplexer multiplexer,
        IOptions<RedisGpsStreamOptions> options,
        ILogger<RedisGpsStreamPublisher> logger)
    {
        _multiplexer = multiplexer;
        _database = multiplexer.GetDatabase();
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync(GpsPositionReceived @event, CancellationToken cancellationToken = default)
    {
        if (@event is null) throw new ArgumentNullException(nameof(@event));

        var payload = JsonSerializer.Serialize(@event, _jsonOptions);

        var entries = new List<NameValueEntry>
        {
            new("payload", payload)
        };

        var streamKey = _options.StreamKey;

        try
        {
            await _database.StreamAddAsync(
                streamKey,
                entries.ToArray(),
                messageId: null,
                maxLength: _options.MaxLength,
                useApproximateMaxLength: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publicando evento GPS al Redis Stream {StreamKey}", streamKey);
            throw;
        }
    }
}
