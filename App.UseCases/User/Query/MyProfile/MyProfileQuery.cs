using App.Objects.User.DTOs.Output.Response;
using App.Shared.Result;
using Cortex.Mediator.Queries;

namespace App.UseCases.User.Query.MyProfile;

public class MyProfileQuery : IQuery<OutputPort<ProfileResponseDto>>
{
    public Guid UserId { get; }
    public MyProfileQuery(Guid userId)
    {
        UserId = userId;
    }
}