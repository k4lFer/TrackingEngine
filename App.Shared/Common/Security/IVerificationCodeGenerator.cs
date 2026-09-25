namespace App.Shared.Common.Security;

public interface IVerificationCodeGenerator
{
    string GenerateLinkToken();
    string GenerateOtpCode();
}