using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Geofences.Query.GetAll;

public class GetAllGeofenceQuery : IQuery<OutputPort<QueryResult<GeofenceResponse>>>
{
}