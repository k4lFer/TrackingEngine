using App.Objects.Routes.DTOs.Input.Command;
using App.Shared.Result;
using App.Shared.Validation;

namespace App.UseCases.Routes.Command.Update;

public class UpdateRouteCommandValidator : IInputValidator<UpdateRouteRequest>
{
    private readonly List<MessageDto> _messages = [];

    public System.Net.HttpStatusCode StatusCode { get; private set; }

    public IReadOnlyCollection<MessageDto> Messages => _messages;

    public async Task<bool> ValidateAsync(UpdateRouteRequest input, CancellationToken cancellationToken = default)
    {
        _messages.Clear();

        if (input is null)
        {
            _messages.Add(new MessageDto("NULL_INPUT", "Los datos de la ruta no pueden ser nulos."));
            StatusCode = System.Net.HttpStatusCode.BadRequest;
            return false;
        }

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            _messages.Add(new MessageDto("NAME_REQUIRED", "El nombre de la ruta es obligatorio."));
        }

        if (input.Waypoints is null || input.Waypoints.Count < 2)
        {
            _messages.Add(new MessageDto("INVALID_ROUTE_GEOMETRY", "Se requieren al menos 2 puntos para la ruta."));
        }

        if (_messages.Any())
        {
            StatusCode = System.Net.HttpStatusCode.UnprocessableEntity;
            return false;
        }
        
        return true;
    }
}