using IdentityService.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityService.Repositories.IRepository
{
    public interface IRoleRepository : IBaseRepository<RoleEntity>
    {
        Task<RoleEntity?> RoleNameExisted(string roleName);
        Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, Guid? userId);
    }
}