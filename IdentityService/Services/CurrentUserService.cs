using IdentityService.Models;
using System.Security.Claims;
using IdentityService.Services.IService;

namespace IdentityService.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public CurrentUserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public CurrentUser? User
        {
            get
            {
                var user = _contextAccessor.HttpContext?.User;
                if (user?.Identity?.IsAuthenticated != true)
                    return null;

                return new CurrentUser
                {
                    UserId = GetUserId(user),
                    Email = GetClaim(user, ClaimTypes.Email, "email"),
                    UserName = GetClaim(
                        user,
                        ClaimTypes.Name,
                        "name",
                        "preferred_username"),

                    Roles = user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList(),
                    Permissions = user.FindAll("permission").Select(x => x.Value).ToList()
                };
            }
        }

        private static Guid? GetUserId(ClaimsPrincipal user)
        {
            var value = GetClaim(
                user,
                ClaimTypes.NameIdentifier,
                "sub"
            );

            return Guid.TryParse(value, out var userId)
                ? userId
                : null;
        }

        private static string? GetClaim(
            ClaimsPrincipal user,
            params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = user.FindFirst(claimType)?.Value;

                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }

            return null;
        }
    }
}