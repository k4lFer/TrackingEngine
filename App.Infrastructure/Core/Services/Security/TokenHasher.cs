using System.Security.Cryptography;
using System.Text;
using App.Shared.Security;

namespace App.Infrastructure.Core.Services.Security;

public class TokenHasher : ITokenHasher
{
    public string Hash(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
}