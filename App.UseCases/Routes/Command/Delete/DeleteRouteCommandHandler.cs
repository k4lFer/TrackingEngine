using System.Net;
using App.Interfaces.Ports.Routes;
using App.Objects.Routes.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Routes.Command.Delete;

public class DeleteRouteCommandHandler : ICommandHandler<DeleteRouteCommand, OutputPort<RouteResponse>>
{
    private readonly IRouteRepository _routeRepository;

    public DeleteRouteCommandHandler(IRouteRepository routeRepository)
    {
        _routeRepository = routeRepository;
    }

    public async Task<OutputPort<RouteResponse>> Handle(DeleteRouteCommand command, CancellationToken cancellationToken)
    {
        var entity = await _routeRepository.GetByIdRawAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<RouteResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró la ruta.", "ROUTE_NOT_FOUND"));
        }

        _routeRepository.Remove(entity);
        await _routeRepository.SaveChangesAsync(cancellationToken);

        return OutputPort<RouteResponse>.Success(data: null, statusCode: HttpStatusCode.NoContent, message: "Ruta eliminada correctamente.");
    }
}