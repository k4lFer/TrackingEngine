using App.Domain.User.Entities;

namespace App.Interfaces.Ports.User;

public interface IUserRepository : IBaseRepository<TUser>
{
    Task<TUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}