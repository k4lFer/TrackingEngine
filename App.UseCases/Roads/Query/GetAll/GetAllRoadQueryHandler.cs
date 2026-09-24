using App.Interfaces.Ports.Roads;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using App.UseCases.Roads.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.Roads.Query.GetAll;

public class GetAllRoadQueryHandler : IQueryHandler<GetAllRoadQuery, OutputPort<QueryResult<RoadResponse>>>
{
    private readonly IRoadQueryRepository _roadQueryRepository;

    public GetAllRoadQueryHandler(IRoadQueryRepository roadQueryRepository)
    {
        _roadQueryRepository = roadQueryRepository;
    }

    public async Task<OutputPort<QueryResult<RoadResponse>>> Handle(GetAllRoadQuery query, CancellationToken cancellationToken)
    {
        var filter = new FilterAllRoads
        {
            Search = query.Filter.Search,
        };

        var results = await _roadQueryRepository.GetRoadsPagedAsync(
            query.Filter.NumberPage,
            query.Filter.PageSize,
            filter,
            cancellationToken);

        return OutputPort<QueryResult<RoadResponse>>.Success(data: results);
    }
}