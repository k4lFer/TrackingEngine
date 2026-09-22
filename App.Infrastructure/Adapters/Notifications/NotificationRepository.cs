using App.Domain.Notifications.Entities;
using App.Infrastructure.Core.DataBaseContext.Connection;
using App.Interfaces.Ports.Notifications;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Adapters.Notifications;

public class NotificationRepository : BaseRepository<TNotification>, INotificationRepository
{
    public NotificationRepository(AppDataBaseContext dbc) : base(dbc) { }

    public async Task<TNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbc.Notifications.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TNotification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbc.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
