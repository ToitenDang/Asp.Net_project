using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Text.Json;

public class CustomResponseAuthorizationMiddleware : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // Nếu user đã đăng nhập nhưng không qua được handler (không gọi context.Succeed)
        if (authorizeResult.Forbidden)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message = "You don't have permission!",
                error_code = "PERMISSION_DENIED"
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            // Dung pipeline khong goi toi middleware ke tiep.
            return;
        }

        // Chua dang nhap (Unauthorized)
        if (authorizeResult.Challenged)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new { success = false, message = "Please log in to continue." };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        // Tiep tuc request
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }
}