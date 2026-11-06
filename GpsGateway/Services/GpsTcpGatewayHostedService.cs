using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
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
                Console.WriteLine($"[GW] accepted {client.Client.RemoteEndPoint}");
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
                Console.WriteLine($"[GW] read={read} buf={buffer.Count}");

                    if (buffer.Count > MaxBufferBytes)
                    {
                        _logger.LogWarning(
                            "[gps:tcp] conexiÃ³n descartada: {Bytes} bytes sin separador de trama (lÃ­mite {Limit}).",
                            buffer.Count, MaxBufferBytes);
                        break;
                    }

                    while (TryExtractFrame(buffer, out var frame))
                    {
                        var raw = Encoding.ASCII.GetString(frame).TrimEnd('\r', '\n');
                        Console.WriteLine($"[GW] frame={raw}");
                        if (raw.StartsWith("##,imei:", StringComparison.Ordinal) || raw.StartsWith("imei:", StringComparison.Ordinal) || Regex.IsMatch(raw, "^[0-9]{9,15};$") || raw.StartsWith(",imei:", StringComparison.Ordinal))
                        {
                            await TryHandleTk103HandshakeAsync(client, frame, stoppingToken);
                            continue;
                        }

                        if (raw.StartsWith("GET ", StringComparison.OrdinalIgnoreCase) ||
                            raw.StartsWith("POST ", StringComparison.OrdinalIgnoreCase) ||
                            raw.StartsWith("PUT ", StringComparison.OrdinalIgnoreCase) ||
                            raw.StartsWith("HEAD ", StringComparison.OrdinalIgnoreCase) ||
                            raw.Contains("HTTP/") ||
                            raw.Contains("User-Agent:") ||
                            raw.Contains("sec-ch-ua") ||
                            raw.Contains("Connection:") ||
                            raw.Contains("Host:"))
                        {
                            continue;
                        }

                        if (string.IsNullOrEmpty(raw) || raw == "#")
                        {
                            continue;
                        }

                        var ev = new GpsPositionReceived
                        {
                            RawFrame = raw,
                            Protocol = "tcp",
                            Source = "tcp",
                            ReceivedAtUtc = DateTime.UtcNow
                        };

                        try
                        {
                            await _publisher.PublishAsync(ev, stoppingToken);
                            Console.WriteLine($"[GW] published len={raw.Length} raw={raw}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "[GW] publish failed");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al atender conexiÃ³n GPS: {Message}", ex.Message);
            }
        }
    }

    private static bool TryExtractFrame(List<byte> buffer, out byte[] frame)
    {
        frame = [];
        if (buffer.Count == 0) return false;

        // Skip leading CR/LF
        while (buffer.Count > 0 && (buffer[0] == (byte)'\r' || buffer[0] == (byte)'\n'))
            buffer.RemoveAt(0);
        if (buffer.Count == 0) return false;

        var text = Encoding.ASCII.GetString(buffer.ToArray());

        // TK103 position frame: *HQ...#
        var star = text.IndexOf("*HQ", StringComparison.Ordinal);
        if (star >= 0)
        {
            var hash = text.IndexOf('#', star);
            if (hash >= 0)
            {
                frame = buffer.Skip(star).Take(hash - star + 1).ToArray();
                buffer.RemoveRange(0, star + hash - star + 1);
                return true;
            }
            // incomplete *HQ frame, wait for more data unless buffer is junk before star
            if (star > 0) { buffer.RemoveRange(0, star); }
            return false;
        }

        // TK103 handshake: ##,imei:...; or ,imei:...; (corrupted split)
        var hs1 = text.IndexOf("##,imei:", StringComparison.Ordinal);
        var hs1b = text.IndexOf(",imei:", StringComparison.Ordinal);
        if (hs1b >= 0 && (hs1 < 0 || hs1b < hs1)) hs1 = hs1b;
        if (hs1 >= 0)
        {
            var semi = text.IndexOf(';', hs1);
            if (semi >= 0)
            {
                frame = buffer.Skip(hs1).Take(semi - hs1 + 1).ToArray();
                buffer.RemoveRange(0, hs1 + semi - hs1 + 1);
                return true;
            }
            if (hs1 > 0) buffer.RemoveRange(0, hs1);
            return false;
        }

        // Second handshake: <digits>;
        var m = Regex.Match(text, @"\b(\d{9,15});");
        if (m.Success)
        {
            frame = buffer.Skip(m.Index).Take(m.Length).ToArray();
            buffer.RemoveRange(0, m.Index + m.Length);
            return true;
        }

        // Generic: cut at # or first semicolon/newline if it looks like garbage
        for (var i = 0; i < buffer.Count; i++)
        {
            var b = buffer[i];
            if (b == (byte)'#' || b == (byte)'\n')
            {
                frame = buffer.Take(i + 1).ToArray();
                buffer.RemoveRange(0, i + 1);
                return true;
            }
        }
        if (buffer.Count > MaxBufferBytes) buffer.Clear();
        return false;
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

        Console.WriteLine($"[GW] handshake ack={Encoding.ASCII.GetString(ack)} for={text.Trim()}");
        await stream.WriteAsync(ack, cancellationToken);
        await stream.FlushAsync(cancellationToken);
        return true;
    }
}



