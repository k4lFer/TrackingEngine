using System.Net;
using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Materials.Command.Delete;

public class DeleteMaterialCommandHandler : ICommandHandler<DeleteMaterialCommand, OutputPort<MaterialResponse>>
{
    private readonly IMaterialRepository _materialRepository;

    public DeleteMaterialCommandHandler(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<MaterialResponse>> Handle(DeleteMaterialCommand command, CancellationToken cancellationToken)
    {
        var entity = await _materialRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<MaterialResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el material indicado.", "MATERIAL_NOT_FOUND"));
        }

        _materialRepository.Remove(entity);
        await _materialRepository.SaveChangesAsync(cancellationToken);

        var response = new MaterialResponse(entity.Id, entity.Code, entity.Name, entity.Unit, entity.Active);
        return OutputPort<MaterialResponse>.Success(data: response, statusCode: HttpStatusCode.NoContent, message: "Material eliminado correctamente.");
    }
}