using System.Net;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Geofences.Query.GetById;

public class GetByIdGeofenceQueryHandler : IQueryHandler<GetByIdGeofenceQuery, OutputPort<GeofenceResponse>>
{
    private readonly IGeofenceRepository _geofenceRepository;

    public GetByIdGeofenceQueryHandler(IGeofenceRepository geofenceRepository)
    {
        _geofenceRepository = geofenceRepository;
    }

    public async Task<OutputPort<GeofenceResponse>> Handle(GetByIdGeofenceQuery query, CancellationToken cancellationToken)
    {
        var geofence = await _geofenceRepository.GetByIdAsync(query.Id, cancellationToken);
        if (geofence is null)
        {
            return OutputPort<GeofenceResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el geofence.", "GEOFENCE_NOT_FOUND"));
        }

        return OutputPort<GeofenceResponse>.Success(data: new GeofenceResponse(
            geofence.Id, geofence.Code, geofence.Name, geofence.Kind.ToString(), geofence.Priority,
            GeoJsonConverter.ToGeoJson(geofence.Geometry), geofence.MaxSpeedKmh, geofence.Color, geofence.Active));
    }
}