using System.Net;
using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Materials.Query.GetById;

public class GetByIdMaterialQueryHandler : IQueryHandler<GetByIdMaterialQuery, OutputPort<MaterialResponse>>
{
    private readonly IMaterialRepository _materialRepository;

    public GetByIdMaterialQueryHandler(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<MaterialResponse>> Handle(GetByIdMaterialQuery query, CancellationToken cancellationToken)
    {
        var material = await _materialRepository.GetByIdAsync(query.Id, cancellationToken);
        if (material is null)
        {
            return OutputPort<MaterialResponse>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto("No se encontró el material.", "MATERIAL_NOT_FOUND"));
        }

        return OutputPort<MaterialResponse>.Success(data: new MaterialResponse(material.Id, material.Code, material.Name, material.Unit, material.Active));
    }
}