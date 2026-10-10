namespace App.Objects.Gps.DTOs.Output.Response;

public sealed record GpsIngestResult(
    string Transport,
    string? Decoder,
    int Accepted,
    int Skipped,
    int Suppressed = 0);