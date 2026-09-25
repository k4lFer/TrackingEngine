using System.Net;
using App.Interfaces.Ports.Roads;
using App.Objects.Roads.DTOs.Output.Response;
using App.Shared.Utils.Geometry;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Roads.Command.Delete;

public class DeleteRoadCommandHandler : ICommandHandler<DeleteRoadCommand, OutputPort<RoadResponse>>
{
    private readonly IRoadRepository _roadRepository;

    public DeleteRoadCommandHandler(IRoadRepository roadRepository)
    {
        _roadRepository = roadRepository;
    }

    public async Task<OutputPort<RoadResponse>> Handle(DeleteRoadCommand command, CancellationToken cancellationToken)
    {
        var entity = await _roadRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<RoadResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el camino.", "ROAD_NOT_FOUND"));
        }

        _roadRepository.Remove(entity);
        await _roadRepository.SaveChangesAsync(cancellationToken);

        var response = new RoadResponse(
            entity.Id, entity.Code, entity.Name,
            GeoJsonConverter.ToGeoJson(entity.Geometry),
            entity.MaxSpeedKmh, entity.Active, GeometryHelper.LengthKm(entity.Geometry));

        return OutputPort<RoadResponse>.Success(data: response, statusCode: HttpStatusCode.NoContent, message: "Camino eliminado correctamente.");
    }
}