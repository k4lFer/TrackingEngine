using System.Net;
using App.Interfaces.Ports.Roads;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Roads.Query.GetById;

public class GetByIdRoadQueryHandler : IQueryHandler<GetByIdRoadQuery, OutputPort<RoadResponse>>
{
    private readonly IRoadRepository _roadRepository;

    public GetByIdRoadQueryHandler(IRoadRepository roadRepository)
    {
        _roadRepository = roadRepository;
    }

    public async Task<OutputPort<RoadResponse>> Handle(GetByIdRoadQuery query, CancellationToken cancellationToken)
    {
        var road = await _roadRepository.GetByIdAsync(query.Id, cancellationToken);
        if (road is null)
        {
            return OutputPort<RoadResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el camino.", "ROAD_NOT_FOUND"));
        }

        return OutputPort<RoadResponse>.Success(data: new RoadResponse(
            road.Id,
            road.Code,
            road.Name,
            GeoJsonConverter.ToGeoJson(road.Geometry),
            road.MaxSpeedKmh,
            road.Active,
            GeometryHelper.LengthKm(road.Geometry)));
    }
}