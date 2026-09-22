using System.Net;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.User.Query.MyProfile;

public class MyProfileQueryHandler : IQueryHandler<MyProfileQuery, OutputPort<ProfileResponseDto>>{
    private readonly IUserQueryRepository _userQueryRepository;

    public MyProfileQueryHandler(IUserQueryRepository userQueryRepository)
    {
        _userQueryRepository = userQueryRepository;
    }

    public async Task<OutputPort<ProfileResponseDto>> Handle(MyProfileQuery query, CancellationToken cancellationToken)
    {
        var user = await _userQueryRepository.GetMyProfile(query.UserId);
        
        if (user is null) return OutputPort<ProfileResponseDto>.Failure(
            HttpStatusCode.NotFound, 
            new MessageDto(code:"USER_NOT_FOUND",message:"User not found"));

        return OutputPort<ProfileResponseDto>.Success(data:user);
    }
}