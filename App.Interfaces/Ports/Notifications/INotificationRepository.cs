using App.Domain.Notifications.Entities;

namespace App.Interfaces.Ports.Notifications;

public interface INotificationRepository : IBaseRepository<TNotification>
{
    Task<TNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TNotification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
