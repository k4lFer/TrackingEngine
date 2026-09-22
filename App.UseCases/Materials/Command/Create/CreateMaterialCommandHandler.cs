using System.Net;
using App.Domain.Materials.Entities;
using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Commands;

namespace App.UseCases.Materials.Command.Create;

public class CreateMaterialCommandHandler : ICommandHandler<CreateMaterialCommand, OutputPort<MaterialResponse>>
{
    private readonly IMaterialRepository _materialRepository;

    public CreateMaterialCommandHandler(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<MaterialResponse>> Handle(CreateMaterialCommand command, CancellationToken cancellationToken)
    {
        var dto = command.Input;

        if (await _materialRepository.ExistsAsync(dto.Code.Trim(), cancellationToken))
        {
            return OutputPort<MaterialResponse>.Failure(
                HttpStatusCode.Conflict,
                new MessageDto("No se puede crear el material porque el código ya existe.", "MATERIAL_CODE_EXISTS"));
        }

        var entity = TMaterial.Create(
            dto.Code.Trim(),
            dto.Name.Trim(),
            dto.Unit);

        _materialRepository.Add(entity);
        await _materialRepository.SaveChangesAsync(cancellationToken);

        var response = new MaterialResponse(entity.Id, entity.Code, entity.Name, entity.Unit, entity.Active);
        return OutputPort<MaterialResponse>.Success(data: response, statusCode: HttpStatusCode.Created, message: "Material creado correctamente.");
    }
}