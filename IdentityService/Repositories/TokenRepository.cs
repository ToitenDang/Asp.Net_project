using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Repositories.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public TokenRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<RefreshTokenEntity?> GetValidRefreshToken(Guid userId, string refreshToken)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.RefreshToken == refreshToken && x.IsRevoke == true && x.ExpirationTime > DateTime.UtcNow);
        }

        public async Task AddRefreshTokenAsync(RefreshTokenEntity refreshToken)
        {
            if (refreshToken == null) throw new ArgumentNullException(nameof(refreshToken));
            _context.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public Task UpdateRefreshToken(RefreshTokenEntity refreshToken)
        {
            throw new NotImplementedException();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}