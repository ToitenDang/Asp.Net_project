using IdentityService.Models.Response;

namespace IdentityService.Services.IService
{
    public interface IUserService
    {
        Task<ResultResponse> GetAllUsers();
    }
}