using IdentityService.Entities;
using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Repositories.IRepository
{
    public interface IUserRepository
    {
        Task<List<UserEntity>> GetAllUsers();

        Task<UserEntity?> GetByUserNameAsync(string userName);

        Task<UserEntity?> GetByUserIdAsync(Guid userId);

        Task AddAsync(UserEntity user);

        Task AddUserAsync(UserEntity user);

        Task<UserEntity?> GetUserWithRolesAndPermissionsAsync(string userName);

        Task SaveChangesAsync();

        Task AddUserRoleAsync(UserRoleEntity userRole);
    }
}