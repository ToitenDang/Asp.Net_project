using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Services.IService
{
    public interface IUserService
    {
        Task<ResultResponse> GetAllUsers();

        Task<ResultResponse> GetUserById(Guid id);

        Task<ResultResponse> UpdateUserInfo(Guid userId, UserUpdateRequest request);
    }
}