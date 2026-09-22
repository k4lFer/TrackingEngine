using App.Objects.Tracking.DTOs.Output.Response;

namespace App.Interfaces.Ports.Tracking;

public interface ITrackingReadRepository
{
    Task<PositionResponse?> GetLatestPositionAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<List<PositionResponse>> GetLatestPositionByVehicleAsync(
        CancellationToken cancellationToken = default);

    Task<List<PositionResponse>> GetPositionHistoryAsync(
        Guid vehicleId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<List<TrackingEventResponse>> GetRecentEventsAsync(
        Guid vehicleId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<RecentEventsResponse> GetRecentEventsAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<List<TrackingEventResponse>> GetEventsByTripAsync(
        Guid tripId,
        CancellationToken cancellationToken = default);

    Task<TripDetailResponse?> GetLatestActiveTripAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<TripDetailResponse?> GetTripByIdAsync(
        Guid tripId,
        CancellationToken cancellationToken = default);

    Task<List<TripSummaryResponse>> GetTripsAsync(
        Guid vehicleId,
        CancellationToken cancellationToken = default);

    Task<List<TripSummaryResponse>> GetAllTripsAsync(
        CancellationToken cancellationToken = default);
}