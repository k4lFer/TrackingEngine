namespace App.Shared.Security
{
    public interface ICurrentUser
    {
        UserClaims? GetClaim();
    }
}
