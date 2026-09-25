using System.Security.Cryptography;
using App.Shared.Common.Security;

namespace App.Infrastructure.Core.Services.Security;

public class VerificationCodeGenerator : IVerificationCodeGenerator
{
    private const string OtpAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    private const int OtpLength = 6;
    private const int TokenLength = 32;

    public string GenerateLinkToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(TokenLength);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public string GenerateOtpCode()
    {
        Span<char> code = stackalloc char[OtpLength];
        var random = RandomNumberGenerator.GetBytes(OtpLength);

        for (var i = 0; i < OtpLength; i++)
        {
            code[i] = OtpAlphabet[random[i] % OtpAlphabet.Length];
        }

        return new string(code);
    }
}