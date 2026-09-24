using System.Text;
using App.UseCases.Gps;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controller.Gps;

/// <summary>
/// Punto de entrada HTTP para dispositivos GPS que maneja Path Variables para Traccar Client.
/// Configuración en celular: http://tu-ip:puerto/api/gps/report/EL_ID_DE_TU_CELULAR
/// </summary>
[ApiController]
[Route("api/gps")]
[Tags("GPS - Dispositivos")]
[Produces("application/json")]
public class GpsIngestController : ControllerBase
{
    private readonly GpsIngestionService _gps;
    private readonly ILogger<GpsIngestController> _logger; 

    public GpsIngestController(GpsIngestionService gps, ILogger<GpsIngestController> logger)
    {
        _gps = gps;
        _logger = logger;
    }

    [HttpGet("report/{deviceId?}")]
    [AllowAnonymous]
    [EndpointSummary("Reportar posición (OsmAnd / Traccar Client vía GET)")]
    [EndpointDescription("Acepta el ID por ruta y el resto de parámetros OsmAnd por query string")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReportFromQuery(string? deviceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 [GET REQ] Detectado en la ruta. Path: {Path}", Request.Path);
        
        // Traccar Client en GET envía todo en la QueryString
        var queryParams = Request.QueryString.Value?.TrimStart('?') ?? "";
        
        string fullPayload = BuildPayload(deviceId, queryParams);
        
        _logger.LogInformation("📝 [GET PAYLOAD] Combinado final: {Payload}", fullPayload);

        return await IngestAsync(fullPayload, cancellationToken);
    }
/*
    [HttpPost("report/{deviceId?}")]
    [AllowAnonymous]
    [EndpointSummary("Reportar posición (OsmAnd / Traccar Client vía POST)")]
    [EndpointDescription("Lee la Query String obligatoriamente ya que Traccar Client envía los datos en la URL aunque use POST")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReportFromBody(string? deviceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 [POST REQ] Detectado en la ruta. Path: {Path}", Request.Path);

        // LEER LA QUERY STRING: Traccar Client por defecto manda los parámetros en la URL (?id=...&lat=...)
        // aunque la petición use el verbo POST. Su Request.Body suele venir vacío.
        var dataFromUrl = Request.QueryString.Value?.TrimStart('?') ?? "";
        
        // Salvaguarda: Si por alguna razón otra app manda los datos en el Body como texto string normal
        string dataFromBody = string.Empty;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            dataFromBody = (await reader.ReadToEndAsync(cancellationToken)).TrimStart('?');
        }

        // Seleccionamos el origen de datos que contenga información útil
        string activeParams = !string.IsNullOrWhiteSpace(dataFromUrl) ? dataFromUrl : dataFromBody;

        _logger.LogDebug("[POST DEBUG] Origen de Datos -> URL Query: '{UrlData}' | Body: '{BodyData}'", dataFromUrl, dataFromBody);

        string fullPayload = BuildPayload(deviceId, activeParams);
        
        _logger.LogInformation("📝 [POST PAYLOAD] Combinado final: {Payload}", fullPayload);

        return await IngestAsync(fullPayload, cancellationToken);
    }
*/
    
        [HttpPost("report/{deviceId?}")]
    [AllowAnonymous]
    public async Task<IActionResult> ReportFromBody(string? deviceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🚀 [POST REQ] Path: {Path} | HasForm: {HasForm}", Request.Path, Request.HasFormContentType);

        var payloadBuilder = new StringBuilder();

        // 1. Intentar leer si viene como parámetros de Formulario tradicional (k=v&k=v)
        if (Request.HasFormContentType)
        {
            var formFields = Request.Form.Select(f => $"{f.Key}={Uri.EscapeDataString(f.Value.ToString())}");
            payloadBuilder.Append(string.Join("&", formFields));
        }

        // 2. Si el formulario estuvo vacío, intentamos leer el Body crudo
        if (payloadBuilder.Length == 0)
        {
            Request.EnableBuffering();
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                var bodyText = await reader.ReadToEndAsync(cancellationToken);
                payloadBuilder.Append(bodyText.TrimStart('?'));
            }
        }

        // 3. Si sigue vacío, verificamos si los mandó por Query String en el POST
        if (payloadBuilder.Length == 0)
        {
            payloadBuilder.Append(Request.QueryString.Value?.TrimStart('?') ?? "");
        }

        // 4. Inyectar el ID de la ruta si se usó /report/123456
        string finalPayload = payloadBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(deviceId) && !finalPayload.Contains("id="))
        {
            finalPayload = string.IsNullOrWhiteSpace(finalPayload) 
                ? $"id={deviceId}" 
                : $"id={deviceId}&{finalPayload}";
        }

        _logger.LogInformation("📝 [POST PAYLOAD] Combinado final: {Payload}", finalPayload);

        return await IngestAsync(finalPayload, cancellationToken);
    }

    /// <summary>
    /// Une de forma segura el deviceId de la URL con los parámetros de geolocalización
    /// evitando duplicar la clave 'id=' si ya viene incluida.
    /// </summary>
    private string BuildPayload(string? deviceId, string queryParams)
    {
        var payloadBuilder = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(deviceId))
        {
            // Solo inyectamos "id=" si la query string no contiene ya un identificador
            if (!queryParams.Contains("id="))
            {
                payloadBuilder.Append($"id={Uri.EscapeDataString(deviceId)}");
                if (!string.IsNullOrWhiteSpace(queryParams))
                {
                    payloadBuilder.Append($"&{queryParams}");
                }
            }
            else
            {
                payloadBuilder.Append(queryParams);
            }
        }
        else
        {
            payloadBuilder.Append(queryParams);
        }

        return payloadBuilder.ToString();
    }

    private async Task<IActionResult> IngestAsync(string payload, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            _logger.LogWarning("⚠️ [GPS CANCELADO] El payload procesado está completamente vacío. No hay datos para procesar.");
            return Ok(new { decoder = "none", accepted = false, skipped = true });
        }

        try
        {
            _logger.LogInformation("⚙️ [PROCESANDO] Enviando trampa al servicio de ingesta...");
            var result = await _gps.HandleAsync("http", Encoding.UTF8.GetBytes(payload), cancellationToken);
            
            _logger.LogInformation("✅ [PROCESADO] Resultado de Ingesta -> Decoder: {Decoder} | Aceptado: {Accepted} | Omitido: {Skipped}", 
                result.Decoder, result.Accepted, result.Skipped);

            return Ok(new { decoder = result.Decoder, accepted = result.Accepted, skipped = result.Skipped });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ERROR CRÍTICO] Ocurrió una falla procesando el tracking del GPS.");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Falla interna procesando tracking" });
        }
    }
}
