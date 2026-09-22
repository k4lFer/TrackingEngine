using App.Domain.User.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.User;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.User;

public class UserRepository : BaseRepository<TUser>, IUserRepository
{
    public UserRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<TUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<TUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbc.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }
}