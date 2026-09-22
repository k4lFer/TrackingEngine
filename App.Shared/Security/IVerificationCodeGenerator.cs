namespace App.Shared.Security;

public interface IVerificationCodeGenerator
{
    string GenerateLinkToken();
    string GenerateOtpCode();
}