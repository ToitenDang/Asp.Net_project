using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Services.IService
{
    public interface IAuthService
    {
        Task<ResultResponse> Authenticate(AuthenRequest request);

        Task<ResultResponse> Logout(RefreshTokenRequest request);

        Task<ResultResponse> Registered(UserRequest request);

        Task<ResultResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}