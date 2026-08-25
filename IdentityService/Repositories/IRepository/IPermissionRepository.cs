using IdentityService.Entities;

namespace IdentityService.Repositories.IRepository
{
    public interface IPermissionRepository : IBaseRepository<PermissionEntity>
    {
        Task<PermissionEntity?> PermissionNameExisted(string name);

        Task<PermissionEntity?> PermissionCodeExisted(string code);

        Task<bool> IsPermissionUsed(Guid Id);
    }
}