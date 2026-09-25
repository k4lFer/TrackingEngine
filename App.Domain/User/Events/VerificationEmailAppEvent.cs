using App.Shared.Common.Domain;

namespace App.Domain.User.Events;

public sealed record VerificationEmailAppEvent(
    Guid UserId,
    string Email,
    string Username,
    string LinkToken,
    string OtpCode) : BaseAppEvent;