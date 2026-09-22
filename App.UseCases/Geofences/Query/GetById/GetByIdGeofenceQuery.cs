using App.Objects.Geofences.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Geofences.Query.GetById;

public class GetByIdGeofenceQuery : IQuery<OutputPort<GeofenceResponse>>
{
    public Guid Id { get; }

    public GetByIdGeofenceQuery(Guid id) => Id = id;
}