using App.Domain.User.Entities;
using App.Interfaces.Ports.User;
using App.Shared.Common.Enums;
using App.Shared.Common.Security;

namespace App.Infrastructure.Core.Services.Security;

public class VerificationFactory : IVerificationFactory
{
    private const int ExpirationMinutes = 15;

    private readonly IVerificationCodeGenerator _codeGenerator;
    private readonly ITokenHasher _tokenHasher;

    public VerificationFactory(IVerificationCodeGenerator codeGenerator, ITokenHasher tokenHasher)
    {
        _codeGenerator = codeGenerator;
        _tokenHasher = tokenHasher;
    }

    public VerificationResult CreateVerification(Guid userId, string email, VerificationType type)
    {
        var linkToken = _codeGenerator.GenerateLinkToken();
        var otpCode = _codeGenerator.GenerateOtpCode();
        var expiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes);

        var verification = TEmailVerification.Create(
            userId,
            email,
            tokenHash: _tokenHasher.Hash(linkToken),
            codeHash: _tokenHasher.Hash(otpCode),
            type,
            linkExpiresAt: expiresAt,
            codeExpiresAt: expiresAt
        );

        return new VerificationResult(verification, linkToken, otpCode);
    }
}