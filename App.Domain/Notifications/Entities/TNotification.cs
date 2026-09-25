using App.Shared.Common.Enums;

namespace App.Domain.Notifications.Entities;

public class TNotification
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public NotificationType Type { get; private set; }
    public string Title { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public bool IsRead { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? SentAt { get; private set; }

    private TNotification() { }

    private TNotification(Guid userId, NotificationType type, string title, string content)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Type = type;
        Title = title;
        Content = content;
        IsRead = false;
        CreatedAt = DateTime.UtcNow;
        SentAt = null;
    }

    public static TNotification Create(Guid userId, NotificationType type, string title, string content)
    {
        return new TNotification(userId, type, title, content);
    }

    public void MarkAsSent()
    {
        SentAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
