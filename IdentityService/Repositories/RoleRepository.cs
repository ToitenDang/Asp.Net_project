using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Repositories
{
    public class RoleRepository : BaseRepository<RoleEntity>, IRoleRepository
    {
        public RoleRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<RoleEntity?> RoleCodeExisted(string roleCode)
        {
            var res = await _context.Roles.FirstOrDefaultAsync(x => x.RoleCode == roleCode);
            return res;
        }

        public async Task UpdateRolePermissionsAsync(Guid roleId, List<Guid> permissionIds, Guid? userId)
        {
            var existingRolePermissions = await _context.Set<RolePermissionEntity>()
                .Where(x => x.RoleId == roleId)
                .ToListAsync();

            if (existingRolePermissions.Any())
            {
                _context.Set<RolePermissionEntity>().RemoveRange(existingRolePermissions);
            }

            if (permissionIds != null && permissionIds.Any())
            {
                var newRolePermissions = permissionIds.Select(permissionId => new RolePermissionEntity
                {
                    Id = Guid.NewGuid(),
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });

                await _context.Set<RolePermissionEntity>().AddRangeAsync(newRolePermissions);
            }
        }
    }
}