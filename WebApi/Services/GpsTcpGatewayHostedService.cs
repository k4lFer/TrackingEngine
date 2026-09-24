using System.Net.Sockets;
using System.Text;
using App.UseCases.Gps;

namespace WebApi.Services;

/// <summary>
/// Gateway TCP genérico para dispositivos GPS (por defecto puerto 8090).
/// Acepta conexiones de trackers que hablan por socket:
///  - TK103 sobre GPRS: tramas "*HQ,...,#" y login "##,imei:...,A;" / "imei:...;".
//  - Cualquier protocolo de texto: la trama se reenvía al servicio de ingesta
///    (que prueba cada decodificador registrado).
/// Config: Gps:TcpPort (por defecto 8090) y Gps:Enabled.
/// </summary>
public sealed class GpsTcpGatewayHostedService : BackgroundService
{
    private readonly int _port;
    private readonly bool _enabled;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<GpsTcpGatewayHostedService> _logger;

    public GpsTcpGatewayHostedService(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<GpsTcpGatewayHostedService> logger)
    {
        _port = configuration.GetValue("Gps:TcpPort", 8090);
        _enabled = configuration.GetValue("Gps:Enabled", true);
        _scopeFactory = scopeFactory;
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

                    while (TryExtractFrame(buffer, out var frame))
                    {
                        if (await TryHandleTk103HandshakeAsync(client, frame, stoppingToken))
                        {
                            continue;
                        }

                        using var scope = _scopeFactory.CreateScope();
                        var gps = scope.ServiceProvider.GetRequiredService<GpsIngestionService>();
                        var result = await gps.HandleAsync("tcp", frame, stoppingToken);
                        _logger.LogInformation("[gps:tcp] decoder={Decoder} aceptadas={Accepted} omitidas={Skipped}", result.Decoder, result.Accepted, result.Skipped);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al atender conexión GPS: {Message}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Extrae trama del buffer: termina en '#' (frame de posición TK103) o en
    /// ';' (mensajes de login/heartbeat TK103).
    /// </summary>
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