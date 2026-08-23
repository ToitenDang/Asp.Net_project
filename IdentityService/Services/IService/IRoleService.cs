using IdentityService.Models.Request;
using IdentityService.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Services.IService
{
    public interface IRoleService
    {
        Task<ResultResponse> CreateRole(RoleRequest request);

        Task<ResultResponse> UpdateRole(Guid roleId, RoleRequest request);

        Task<ResultResponse> DeleteRole(Guid roleId);

        Task<ResultResponse> GetAllRoles();

        Task<ResultResponse> GetById(Guid Id);
    }
}