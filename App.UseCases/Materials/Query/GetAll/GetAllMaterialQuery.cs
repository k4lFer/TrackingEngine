using App.Objects.Materials.DTOs.Input.Query;
using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Materials.Query.GetAll;

public class GetAllMaterialQuery : IQuery<OutputPort<QueryResult<MaterialResponse>>>
{
    public MaterialFilterDto Filter { get; }

    public GetAllMaterialQuery(MaterialFilterDto filter)
    {
        Filter = filter;
    }
}