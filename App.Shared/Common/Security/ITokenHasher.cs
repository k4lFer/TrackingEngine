namespace App.Shared.Common.Security;

public interface ITokenHasher
{
    string Hash(string token);
}