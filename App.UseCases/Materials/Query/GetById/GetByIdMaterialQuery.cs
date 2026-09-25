using App.Objects.Materials.DTOs.Output.Response;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.Materials.Query.GetById;

public class GetByIdMaterialQuery : IQuery<OutputPort<MaterialResponse>>
{
    public Guid Id { get; }

    public GetByIdMaterialQuery(Guid id) => Id = id;
}