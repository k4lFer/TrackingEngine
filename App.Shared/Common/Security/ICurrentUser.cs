namespace App.Shared.Common.Security
{
    public interface ICurrentUser
    {
        UserClaims? GetClaim();
    }
}
