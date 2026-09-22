using System.Security.Claims;
using App.Shared.Security;

namespace WebApi
{
    public class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Assuming the ICurrentUser interface can be changed to not require HttpContext as a parameter
        public UserClaims? GetClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // Note: Your JWT configuration uses RoleClaimType = ClaimTypes.Role.
            // You should be consistent in how you create and read claims.
            Guid id = Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            //if (id == Guid.Empty || string.IsNullOrEmpty(role)) return null;

            return new UserClaims(id, role);
        }
    }
}