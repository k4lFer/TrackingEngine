using System.Net.Sockets;
using System.Text;
using App.Interfaces.Ports.Messaging;
using App.Objects.Gps.Events;

namespace GpsGateway.Services;

public sealed class GpsTcpGatewayHostedService : BackgroundService
{
    private readonly int _port;
    private readonly bool _enabled;
    private readonly IGpsEventPublisher _publisher;
    private readonly ILogger<GpsTcpGatewayHostedService> _logger;

    public GpsTcpGatewayHostedService(
        IConfiguration configuration,
        IGpsEventPublisher publisher,
        ILogger<GpsTcpGatewayHostedService> logger)
    {
        _port = configuration.GetValue("Gps:TcpPort", 8090);
        _enabled = configuration.GetValue("Gps:Enabled", true);
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_enabled)
        {
            _logger.LogInformation("Gateway TCP GPS deshabilitado (Gps:Enabled=false).");
            return;
        }

        var listener = new TcpListener(System.Net.IPAddress.Any, _port);
        try
        {
            listener.Start();
            _logger.LogInformation("Gateway TCP GPS escuchando en 0.0.0.0:{Port}", _port);

            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                _ = Task.Run(() => HandleClientAsync(client, stoppingToken), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Apagado normal.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en el gateway TCP GPS.");
        }
        finally
        {
            listener.Stop();
        }
    }

    private const int MaxBufferBytes = 64 * 1024;

    private async Task HandleClientAsync(TcpClient client, CancellationToken stoppingToken)
    {
        using (client)
        using (var stream = client.GetStream())
        {
            var buffer = new List<byte>();
            var chunk = new byte[4096];

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var read = await stream.ReadAsync(chunk, stoppingToken);
                    if (read == 0) break;

                    for (var i = 0; i < read; i++) buffer.Add(chunk[i]);

                    if (buffer.Count > MaxBufferBytes)
                    {
                        _logger.LogWarning(
                            "[gps:tcp] conexión descartada: {Bytes} bytes sin separador de trama (límite {Limit}).",
                            buffer.Count, MaxBufferBytes);
                        break;
                    }

                    while (TryExtractFrame(buffer, out var frame))
                    {
                        if (await TryHandleTk103HandshakeAsync(client, frame, stoppingToken))
                        {
                            continue;
                        }

                        var raw = Encoding.ASCII.GetString(frame);
                        var ev = new GpsPositionReceived
                        {
                            RawFrame = raw,
                            Protocol = "tcp",
                            Source = "tcp",
                            ReceivedAtUtc = DateTime.UtcNow
                        };

                        await _publisher.PublishAsync(ev, stoppingToken);

                        _logger.LogDebug("[gps:tcp] trama publicada al stream Redis (len={Len})", frame.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al atender conexión GPS: {Message}", ex.Message);
            }
        }
    }

    private static bool TryExtractFrame(List<byte> buffer, out byte[] frame)
    {
        frame = [];
        var end = -1;
        for (var i = 0; i < buffer.Count; i++)
        {
            if (buffer[i] == (byte)'#' || buffer[i] == (byte)';')
            {
                end = i;
                break;
            }
        }
        if (end < 0) return false;

        frame = buffer.Take(end + 1).ToArray();
        buffer.RemoveRange(0, end + 1);
        return true;
    }

    private static async Task<bool> TryHandleTk103HandshakeAsync(
        TcpClient client,
        byte[] frame,
        CancellationToken cancellationToken)
    {
        var text = Encoding.ASCII.GetString(frame);
        var stream = client.GetStream();
        var ack = text switch
        {
            _ when text.StartsWith("##,imei:", StringComparison.Ordinal) => "LOAD"u8.ToArray(),
            _ when text.StartsWith("imei:", StringComparison.Ordinal) => "ON"u8.ToArray(),
            _ when System.Text.RegularExpressions.Regex.IsMatch(text, "^[0-9]{9,15};$") => "ON"u8.ToArray(),
            _ => null,
        };

        if (ack is null) return false;

        await stream.WriteAsync(ack, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        return true;
    }
}