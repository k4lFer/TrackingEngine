using App.Objects.User.DTOs.Input.Query;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Common.Query;
using App.Shared.Common.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.User.Query.GetAll;

public class GetAllUserQuery : IQuery<OutputPort<QueryResult<UsersResponseDto>>>
{
    public GetAllDto Input { get; }
    
    public GetAllUserQuery(GetAllDto input)
    {
        Input = input;
    }
}