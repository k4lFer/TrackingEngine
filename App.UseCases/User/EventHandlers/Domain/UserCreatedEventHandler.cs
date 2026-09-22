using App.Domain.User.Events;
using App.Interfaces.Ports;
using App.Interfaces.Ports.User;
using App.Shared.Objects.Enums;
using Cortex.Mediator;
using Cortex.Mediator.Notifications;
using Microsoft.Extensions.Logging;

namespace App.UseCases.User.EventHandlers.Domain;

public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    private readonly IVerificationFactory _verificationFactory;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ILogger<UserCreatedEventHandler> _logger;

    public UserCreatedEventHandler(
        IVerificationFactory verificationFactory,
        IVerificationRepository verificationRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        ILogger<UserCreatedEventHandler> logger)
    {
        _verificationFactory = verificationFactory;
        _verificationRepository = verificationRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var result = _verificationFactory.CreateVerification(
            notification.UserId,
            notification.Email,
            VerificationType.EmailConfirmation
        );

        await _verificationRepository.AddAsync(result.Verification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogInformation("Email verification created for user {UserId}", notification.UserId);

        var appEvent = new VerificationEmailAppEvent(
            notification.UserId,
            notification.Email,
            notification.Username,
            result.LinkToken,
            result.OtpCode
        );

        await _mediator.PublishAsync(appEvent, cancellationToken);
    }
}