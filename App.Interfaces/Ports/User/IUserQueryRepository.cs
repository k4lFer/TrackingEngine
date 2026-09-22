using App.Objects.User.DTOs.Output.Response;
using App.Shared.Query;

namespace App.Interfaces.Ports.User;

public interface IUserQueryRepository
{
    Task<QueryResult<UsersResponseDto>> GetUsersPaged(int page, int pageSize, QueryFilter<UsersResponseDto>? filter = null);
    Task<ProfileResponseDto?> GetMyProfile(Guid id);
}