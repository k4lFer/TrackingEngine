using System.Net;
using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Materials.Query.GetAll;

public class GetAllMaterialQueryHandler : IQueryHandler<GetAllMaterialQuery, OutputPort<QueryResult<MaterialResponse>>>
{
    private readonly IMaterialRepository _materialRepository;

    public GetAllMaterialQueryHandler(IMaterialRepository materialRepository)
    {
        _materialRepository = materialRepository;
    }

    public async Task<OutputPort<QueryResult<MaterialResponse>>> Handle(GetAllMaterialQuery query, CancellationToken cancellationToken)
    {
        var materials = await _materialRepository.GetAllAsync(cancellationToken);

        var results = materials.Select(m => new MaterialResponse(m.Id, m.Code, m.Name, m.Unit, m.Active)).ToList();

        return OutputPort<QueryResult<MaterialResponse>>.Success(
            data: QueryResult<MaterialResponse>.Success(results, results.Count, 1, 1, results.Count));
    }
}