using System.Net;
using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Result;
using Cortex.Mediator.Queries;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace App.UseCases.Geofences.Query.CheckPoint;

public class CheckPointGeofenceQueryHandler : IQueryHandler<CheckPointGeofenceQuery, OutputPort<GeofenceResponse>>
{
    private readonly IGeofenceRepository _geofenceRepository;
    private static readonly GeometryFactory _gf = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    public CheckPointGeofenceQueryHandler(IGeofenceRepository geofenceRepository)
    {
        _geofenceRepository = geofenceRepository;
    }

    public async Task<OutputPort<GeofenceResponse>> Handle(CheckPointGeofenceQuery query, CancellationToken cancellationToken)
    {
        var point = _gf.CreatePoint(new Coordinate(query.Lon, query.Lat));

        var geofence = await _geofenceRepository.GetFirstContainingAsync(point, cancellationToken);
        if (geofence is null)
        {
            return OutputPort<GeofenceResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("Ningún geofence contiene el punto indicado.", "GEOFENCE_NOT_FOUND"));
        }

        return OutputPort<GeofenceResponse>.Success(data: new GeofenceResponse(
            geofence.Id, geofence.Code, geofence.Name, geofence.Kind.ToString(), geofence.Priority,
            GeoJsonConverter.ToGeoJson(geofence.Geometry), geofence.MaxSpeedKmh, geofence.Color, geofence.Active));
    }
}