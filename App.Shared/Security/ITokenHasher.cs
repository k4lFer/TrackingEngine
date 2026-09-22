namespace App.Shared.Security;

public interface ITokenHasher
{
    string Hash(string token);
}