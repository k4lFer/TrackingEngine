using System.Net;
using App.Domain.User.Events;
using App.Interfaces.Ports;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Input.Command;
using App.Shared.Objects.Enums;
using App.Shared.Result;
using App.Shared.Validation;
using Cortex.Mediator;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.ResendVerification;

public class ResendVerificationCommandHandler : ICommandHandler<ResendVerificationCommand, OutputPort<object>>
{
    private readonly IVerificationFactory _verificationFactory;
    private readonly IVerificationRepository _verificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IInputValidator<ResendVerificationDto> _validator;

    public ResendVerificationCommandHandler(
        IVerificationFactory verificationFactory,
        IVerificationRepository verificationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IInputValidator<ResendVerificationDto> validator)
    {
        _verificationFactory = verificationFactory;
        _verificationRepository = verificationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<OutputPort<object>> Handle(ResendVerificationCommand command, CancellationToken cancellationToken)
    {
        if (!await _validator.ValidateAsync(command.Input, cancellationToken))
        {
            return OutputPort<object>.Failure(_validator.StatusCode, _validator.Messages.ToArray());
        }

        var email = command.Input.Email;
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            return OutputPort<object>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto(code: "USER_NOT_FOUND", message: "User not found.")
            );
        }

        if (user.IsEmailVerified)
        {
            return OutputPort<object>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto(code: "EMAIL_ALREADY_VERIFIED", message: "This email is already verified.")
            );
        }

        var previous = await _verificationRepository.GetActiveByUserIdAsync(user.Id, VerificationType.EmailConfirmation, cancellationToken);
        previous?.Invalidate();

        var result = _verificationFactory.CreateVerification(user.Id, user.Email, VerificationType.EmailConfirmation);

        await _verificationRepository.AddAsync(result.Verification, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        var appEvent = new VerificationEmailAppEvent(
            user.Id,
            user.Email,
            user.Username,
            result.LinkToken,
            result.OtpCode
        );

        await _mediator.PublishAsync(appEvent, cancellationToken);

        return OutputPort<object>.Success(
            data: null,
            statusCode: HttpStatusCode.OK,
            message: "Verification email sent successfully."
        );
    }
}