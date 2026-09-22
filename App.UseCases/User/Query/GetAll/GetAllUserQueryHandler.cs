using System.Net;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Query;
using App.Shared.Result;
using App.UseCases.User.Query.Filter;
using Cortex.Mediator.Queries;

namespace App.UseCases.User.Query.GetAll;

public class GetAllUserQueryHandler : IQueryHandler<GetAllUserQuery, OutputPort<QueryResult<UsersResponseDto>>>
{
    private readonly IUserQueryRepository _userQueryRepository;
    
    public GetAllUserQueryHandler(IUserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }
    
    public async Task<OutputPort<QueryResult<UsersResponseDto>>> Handle(GetAllUserQuery query, CancellationToken cancellationToken)
    {
        FilterAllUsers filter = new()
        {
            Email = query.Input.Email
        };
        
        var results = await _userQueryRepository.GetUsersPaged(query.Input.NumberPage, query.Input.PageSize, filter);
        
        if (!results.Results.Any()) return OutputPort<QueryResult<UsersResponseDto>>.Success(data: null, statusCode: HttpStatusCode.NoContent);

        return OutputPort<QueryResult<UsersResponseDto>>.Success(data: results);
    }
}