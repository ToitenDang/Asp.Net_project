using IdentityService.Entities;
using IdentityService.Models.Request;
using IdentityService.Models.Response;

namespace IdentityService.Repositories.IRepository
{
    public interface ITokenRepository
    {
        Task<RefreshTokenEntity?> GetValidRefreshToken(Guid userId, string refreshToken);

        Task AddRefreshTokenAsync(RefreshTokenEntity refreshToken);

        Task UpdateRefreshToken(RefreshTokenEntity refreshToken);

        Task SaveChangesAsync();
    }
}