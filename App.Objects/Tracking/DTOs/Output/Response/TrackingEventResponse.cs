namespace App.Objects.Tracking.DTOs.Output.Response;

public record TrackingEventResponse(
    Guid Id,
    Guid VehicleId,
    string VehicleCode,
    Guid? TripId,
    string Type,
    string Severity,
    DateTime OccurredAt,
    string? GeofenceName,
    string? PayloadJson
);

public record RecentEventsResponse(
    IReadOnlyList<TrackingEventResponse> Events,
    int TotalCount
);