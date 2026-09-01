using IdentityService.Authorization;
using IdentityService.Models.OptionsPatternModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace IdentityService.Authorization
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly DataSeedingSettings _seedingSettings;
        private readonly IHttpContextAccessor _httpContextAccessor; // Thêm dòng này

        public PermissionHandler(
            IOptions<DataSeedingSettings> seedingSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _seedingSettings = seedingSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // 1. Pass qua luôn nếu là Admin
            var adminRoleName = _seedingSettings.RoleSettings.RoleAdminName;
            if (context.User.IsInRole(adminRoleName))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // 2. Logic kiểm tra "PERSONAL_UPDATE" (Chính chủ)
            if (requirement.Permission == "PERSONAL_UPDATE")
            {
                // Lấy UserID từ Token đang đăng nhập
                var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // Lấy UserID từ URL
                var routeId = _httpContextAccessor.HttpContext?.Request.RouteValues["id"]?.ToString();

                // So sánh, nếu đúng chính chủ thì cho qua
                if (!string.IsNullOrEmpty(currentUserId) && currentUserId == routeId)
                {
                    context.Succeed(requirement);
                    return Task.CompletedTask;
                }
            }

            // 3. Logic kiểm tra Permission thông thường
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