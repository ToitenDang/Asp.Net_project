using IdentityService.Configurations.OptionsPatternModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace IdentityService.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly DataSeedingSettings _seedingSettings;

        public PermissionHandler(IOptions<DataSeedingSettings> seedingSettings)
        {
            _seedingSettings = seedingSettings.Value;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // Lay ten vai tro tu cau hinh, sau doi chi can update trong appsettings.
            var adminRoleName = _seedingSettings.RoleSettings.RoleAdminName;
            // Role Admin su dung toan quyen
            if (context.User.IsInRole(adminRoleName))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var hasPermission = context.User.HasClaim(c =>
                c.Type == "permission" && c.Value == requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}