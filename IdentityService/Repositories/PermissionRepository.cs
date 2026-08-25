using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Repositories.IRepository;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Repositories
{
    public class PermissionRepository : BaseRepository<PermissionEntity>, IPermissionRepository
    {
        public PermissionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> IsPermissionUsed(Guid Id)
        {
            var res = await _context.RolePermissions.AnyAsync(x => x.PermissionId == Id);
            return res;
        }

        public async Task<PermissionEntity?> PermissionCodeExisted(string code)
        {
            var exists = await _context.Permissions.FirstOrDefaultAsync(x => x.PermissionCode == code);
            return exists;
        }

        public async Task<PermissionEntity?> PermissionNameExisted(string name)
        {
            var exists = await _context.Permissions.FirstOrDefaultAsync(x => x.Name == name);
            return exists;
        }
    }
}