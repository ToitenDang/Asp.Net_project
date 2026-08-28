using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Services.IService
{
    public interface IUserService
    {
        Task<ResultResponse> GetAllUsers();

        Task<ResultResponse> UpdateUserInfo(Guid userId, UserUpdateRequest request);
    }
}