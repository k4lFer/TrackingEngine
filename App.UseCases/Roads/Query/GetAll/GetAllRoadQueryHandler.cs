using System.Net;
using App.Interfaces.Ports.Roads;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Geometry;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Roads.Query.GetAll;

public class GetAllRoadQueryHandler : IQueryHandler<GetAllRoadQuery, OutputPort<QueryResult<RoadResponse>>>
{
    private readonly IRoadRepository _roadRepository;

    public GetAllRoadQueryHandler(IRoadRepository roadRepository)
    {
        _roadRepository = roadRepository;
    }

    public async Task<OutputPort<QueryResult<RoadResponse>>> Handle(GetAllRoadQuery query, CancellationToken cancellationToken)
    {
        var roads = await _roadRepository.GetAllActiveAsync(cancellationToken);

        var results = roads.Select(r => new RoadResponse(
            r.Id,
            r.Code,
            r.Name,
            GeoJsonConverter.ToGeoJson(r.Geometry),
            r.MaxSpeedKmh,
            r.Active,
            GeometryHelper.LengthKm(r.Geometry))).ToList();

        return OutputPort<QueryResult<RoadResponse>>.Success(
            data: QueryResult<RoadResponse>.Success(results, results.Count, 1, 1, results.Count));
    }
}