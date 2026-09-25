using System.Net;
using App.Domain.User.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Input.Command;
using App.Shared.Common.Enums;
using App.Shared.Common.Result;
using App.Shared.Common.Security;
using App.Shared.Common.Validation;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.VerifyEmail;

public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand, OutputPort<object>>
{
    private readonly IVerificationRepository _verificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenHasher _tokenHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInputValidator<VerifyEmailDto> _validator;

    public VerifyEmailCommandHandler(
        IVerificationRepository verificationRepository,
        IUserRepository userRepository,
        ITokenHasher tokenHasher,
        IUnitOfWork unitOfWork,
        IInputValidator<VerifyEmailDto> validator)
    {
        _verificationRepository = verificationRepository;
        _userRepository = userRepository;
        _tokenHasher = tokenHasher;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<OutputPort<object>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        if (!await _validator.ValidateAsync(command.Input, cancellationToken))
        {
            return OutputPort<object>.Failure(_validator.StatusCode, _validator.Messages.ToArray());
        }

        var dto = command.Input;

        TEmailVerification? verification;
        var attemptsExceeded = false;
        var viaLink = !string.IsNullOrWhiteSpace(dto.Token);

        if (viaLink)
        {
            verification = await ResolveByTokenAsync(dto.Token, cancellationToken);
        }
        else
        {
            var codeResolution = await ResolveByCodeAsync(dto, cancellationToken);
            verification = codeResolution.Verification;
            attemptsExceeded = codeResolution.AttemptsExceeded;
        }

        if (verification is null)
        {
            return OutputPort<object>.Failure(
                HttpStatusCode.BadRequest,
                attemptsExceeded
                    ? new MessageDto(code: "VERIFICATION_ATTEMPTS_EXCEEDED", message: "Too many failed attempts. Request a new code.")
                    : new MessageDto(code: "INVALID_VERIFICATION", message: "The verification token or code is invalid.")
            );
        }

        var isUsable = viaLink ? verification.IsLinkUsable : verification.IsCodeUsable;

        if (!isUsable)
        {
            return OutputPort<object>.Failure(
                HttpStatusCode.BadRequest,
                new MessageDto(code: "VERIFICATION_EXPIRED", message: "The verification has expired. Request a new one.")
            );
        }

        verification.MarkUsed();

        var user = await _userRepository.GetByIdAsync(verification.UserId, cancellationToken);
        if (user is null)
        {
            return OutputPort<object>.Failure(
                HttpStatusCode.NotFound,
                new MessageDto(code: "USER_NOT_FOUND", message: "User not found.")
            );
        }

        user.MarkEmailVerified();
        await _unitOfWork.SaveChanges(cancellationToken);

        return OutputPort<object>.Success(
            data: null,
            statusCode: HttpStatusCode.OK,
            message: "Email verified successfully."
        );
    }

    private async Task<TEmailVerification?> ResolveByTokenAsync(string token, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHasher.Hash(token);
        return await _verificationRepository.GetActiveByTokenHashAsync(tokenHash, VerificationType.EmailConfirmation, cancellationToken);
    }

    private async Task<CodeResolution> ResolveByCodeAsync(VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email ?? string.Empty, cancellationToken);
        if (user is null)
            return new CodeResolution(null, false);

        var verification = await _verificationRepository.GetActiveByUserIdAsync(user.Id, VerificationType.EmailConfirmation, cancellationToken);
        if (verification is null)
            return new CodeResolution(null, false);

        var codeHash = _tokenHasher.Hash(dto.Code);
        if (!string.Equals(verification.CodeHash, codeHash, StringComparison.OrdinalIgnoreCase))
        {
            verification.RegisterFailedAttempt();
            var exceeded = verification.HasExceededAttempts;
            if (exceeded)
            {
                verification.Invalidate();
            }

            await _unitOfWork.SaveChanges(cancellationToken);
            return new CodeResolution(null, exceeded);
        }

        return new CodeResolution(verification, false);
    }

    private sealed record CodeResolution(TEmailVerification? Verification, bool AttemptsExceeded);
}