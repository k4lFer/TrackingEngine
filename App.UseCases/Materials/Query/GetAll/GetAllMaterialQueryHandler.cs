using App.Interfaces.Ports.Materials;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using App.UseCases.Materials.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.Materials.Query.GetAll;

public class GetAllMaterialQueryHandler : IQueryHandler<GetAllMaterialQuery, OutputPort<QueryResult<MaterialResponse>>>
{
    private readonly IMaterialQueryRepository _materialQueryRepository;

    public GetAllMaterialQueryHandler(IMaterialQueryRepository materialQueryRepository)
    {
        _materialQueryRepository = materialQueryRepository;
    }

    public async Task<OutputPort<QueryResult<MaterialResponse>>> Handle(GetAllMaterialQuery query, CancellationToken cancellationToken)
    {
        var filter = new FilterAllMaterials
        {
            Search = query.Filter.Search,
            Active = query.Filter.Active,
        };

        var results = await _materialQueryRepository.GetMaterialsPagedAsync(
            query.Filter.NumberPage,
            query.Filter.PageSize,
            filter,
            cancellationToken);

        return OutputPort<QueryResult<MaterialResponse>>.Success(data: results);
    }
}