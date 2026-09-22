namespace App.Shared.Objects.Enums;

public enum TrackingEventType
{
    GeofenceEntered = 0,
    GeofenceExited = 1,
    SpeedLimitExceeded = 2,
    SpeedLimitCleared = 3,
    RouteDeviationDetected = 4,
    RouteDeviationCleared = 5,
    LongStopDetected = 6,
    LongStopEnded = 7,
    TripStarted = 8,
    TripCompleted = 9,
    TripCancelled = 10,
    GpsConnectionLost = 11,
    GpsConnectionRestored = 12
}