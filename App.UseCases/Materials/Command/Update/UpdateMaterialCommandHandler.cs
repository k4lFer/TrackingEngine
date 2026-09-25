using System.Net;
using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Materials.Command.Update;

public class UpdateMaterialCommandHandler : ICommandHandler<UpdateMaterialCommand, OutputPort<MaterialResponse>>
{
    private readonly IMaterialRepository _materialRepository;

    public UpdateMaterialCommandHandler(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<MaterialResponse>> Handle(UpdateMaterialCommand command, CancellationToken cancellationToken)
    {
        var entity = await _materialRepository.GetByIdAsync(command.Id, cancellationToken);
        if (entity is null)
        {
            return OutputPort<MaterialResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el material indicado.", "MATERIAL_NOT_FOUND"));
        }

        var dto = command.Input;
        entity.Update(dto.Name.Trim(), dto.Unit, dto.Active);

        _materialRepository.Update(entity);
        await _materialRepository.SaveChangesAsync(cancellationToken);

        var response = new MaterialResponse(entity.Id, entity.Code, entity.Name, entity.Unit, entity.Active);
        return OutputPort<MaterialResponse>.Success(data: response, statusCode: HttpStatusCode.OK, message: "Material actualizado correctamente.");
    }
}