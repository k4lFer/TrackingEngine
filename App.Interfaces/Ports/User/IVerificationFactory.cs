using App.Domain.User.Entities;
using App.Shared.Objects.Enums;

namespace App.Interfaces.Ports.User;

public interface IVerificationFactory
{
    VerificationResult CreateVerification(Guid userId, string email, VerificationType type);
}

public record VerificationResult(TEmailVerification Verification, string LinkToken, string OtpCode);