using App.Interfaces.Ports.Geofences;
using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using App.UseCases.Geofences.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.Geofences.Query.GetAll;

public class GetAllGeofenceQueryHandler : IQueryHandler<GetAllGeofenceQuery, OutputPort<QueryResult<GeofenceResponse>>>
{
    private readonly IGeofenceQueryRepository _geofenceQueryRepository;

    public GetAllGeofenceQueryHandler(IGeofenceQueryRepository geofenceQueryRepository)
    {
        _geofenceQueryRepository = geofenceQueryRepository;
    }

    public async Task<OutputPort<QueryResult<GeofenceResponse>>> Handle(GetAllGeofenceQuery query, CancellationToken cancellationToken)
    {
        var filter = new FilterAllGeofences
        {
            Search = query.Filter.Search,
            Active = query.Filter.Active,
        };

        var results = await _geofenceQueryRepository.GetGeofencesPagedAsync(
            query.Filter.NumberPage,
            query.Filter.PageSize,
            filter,
            cancellationToken);

        return OutputPort<QueryResult<GeofenceResponse>>.Success(data: results);
    }
}