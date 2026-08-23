using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Services.IService
{
    public interface IPermissionService
    {
        Task<ResultResponse> CreatePermission(PermissionRequest request);

        Task<ResultResponse> UpdatePermission(Guid permissonId, PermissionRequest request);

        Task<ResultResponse> GetAllPermissions();

        Task<ResultResponse> GetPermissionById(Guid Id);

        Task<ResultResponse> DeletePermisson(Guid permissionId);
    }
}