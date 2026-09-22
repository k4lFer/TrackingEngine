using App.Shared.Domain;

namespace App.Domain.User.Events;

public sealed record UserCreatedEvent(Guid UserId, string Email, string Username, bool IsEmailConfirmed) : BaseEvent;