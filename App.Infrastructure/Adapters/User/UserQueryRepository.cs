using App.Domain.User.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Output.Response;
using App.Shared.Query;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.User;

public class UserQueryRepository : BaseRepository<TUser>, IUserQueryRepository
{
    public UserQueryRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<ProfileResponseDto?> GetMyProfile(Guid id)
    {
        var result = await _dbc.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new ProfileResponseDto(
                u.Id,
                u.Username,
                u.Email,
                u.Person.FirstName,
                u.Person.LastName,
                u.Person.DateOfBirth,
                u.CreatedAt
            ))
            .FirstOrDefaultAsync();

        return result;
    }


    public async Task<QueryResult<UsersResponseDto>> GetUsersPaged(int page, int pageSize, QueryFilter<UsersResponseDto>? filter = null)
    {
        var query = _dbc.Users
            .AsNoTracking()
            .Select(u => new UsersResponseDto(u.Id, u.Email, u.CreatedAt));

        if (filter is not null)
            query = filter.ApplyFilter(query);

        return await PaginateAsync(query, page, pageSize);
    }
}