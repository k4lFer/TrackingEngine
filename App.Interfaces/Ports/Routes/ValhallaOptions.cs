namespace App.Interfaces.Ports.Routes;

public sealed class ValhallaOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:8002";
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Distancia en línea recta mínima (km) entre el primer y último waypoint para
    /// considerar usar Valhalla. Debajo de este valor se prioriza la red vial interna
    /// de la mina (evita que el trazado local salte a carreteras públicas de OSM).
    /// </summary>
    public double MinSpanKm { get; set; } = 5;

    /// <summary>
    /// Factor máximo permitido entre la ruta calculada y la distancia en línea recta.
    /// Planes externos con desvíos mayores se descartan (por ejemplo, que Valhalla
    /// rutee por caminos públicos lejanos en lugar de la red interna).
    /// </summary>
    public double MaxDetourRatio { get; set; } = 3.5;
}