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

        public async Task<PermissionEntity?> PermissionNameExisted(string name)
        {
            var exists = await _context.Permissions.FirstOrDefaultAsync(x => x.Name == name);
            return exists;
        }
    }
}