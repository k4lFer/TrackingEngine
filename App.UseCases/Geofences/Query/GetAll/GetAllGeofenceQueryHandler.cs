using System.Net;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Geofences.Query.GetAll;

public class GetAllGeofenceQueryHandler : IQueryHandler<GetAllGeofenceQuery, OutputPort<QueryResult<GeofenceResponse>>>
{
    private readonly IGeofenceRepository _geofenceRepository;

    public GetAllGeofenceQueryHandler(IGeofenceRepository geofenceRepository)
    {
        _geofenceRepository = geofenceRepository;
    }

    public async Task<OutputPort<QueryResult<GeofenceResponse>>> Handle(GetAllGeofenceQuery query, CancellationToken cancellationToken)
    {
        var geofences = await _geofenceRepository.GetAllAsync(cancellationToken);

        var results = geofences.Select(g => new GeofenceResponse(
            g.Id, g.Code, g.Name, g.Kind.ToString(), g.Priority,
            GeoJsonConverter.ToGeoJson(g.Geometry), g.MaxSpeedKmh, g.Color, g.Active)).ToList();

        return OutputPort<QueryResult<GeofenceResponse>>.Success(
            data: QueryResult<GeofenceResponse>.Success(results, results.Count, 1, 1, results.Count));
    }
}