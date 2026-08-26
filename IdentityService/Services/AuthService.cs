using AutoMapper;
using FluentValidation;
using IdentityService.Entities;
using IdentityService.Enum;
using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IRedisService _redisService;
        private readonly IMapper _mapper;
        private readonly IValidator<UserRequest> _validator;
        private readonly ITokenRepository _tokenRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public AuthService(IConfiguration configuration, IHttpContextAccessor httpContext, IRedisService redisService, IMapper mapper, ITokenRepository tokenRepository, IRoleRepository roleRepository, IUserRepository userRepository, IValidator<UserRequest> validator)
        {
            _configuration = configuration;
            _httpContext = httpContext;
            _redisService = redisService;
            _mapper = mapper;
            _validator = validator;
            _tokenRepository = tokenRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        #region Register User

        public async Task<ResultResponse> Registered(UserRequest request)
        {
            var res = new ResultResponse<UserResponse>();
            var hash = new PasswordHasher<UserEntity>();
            var roleCodeUser = _configuration["AuthorizationSettings:RoleCodeUser"] ?? "USER";

            var validationResult = await _validator.ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var existedUser = await _userRepository.GetByUserNameAsync(request.UserName);
            if (existedUser != null)
            {
                return new ResultResponse(false, "User existed!");
            }

            var role = await _roleRepository.RoleCodeExisted(roleCodeUser.ToUpper());

            var user = new UserEntity();

            user = _mapper.Map<UserEntity>(request);
            user.Id = Guid.NewGuid();
            user.Password = user.Password != null ? hash.HashPassword(user, user.Password) : null;
            user.CreatedAt = DateTime.UtcNow;

            await _userRepository.AddUserAsync(user);

            if (role != null)
            {
                var userRole = new UserRoleEntity();
                userRole.Id = Guid.NewGuid();
                userRole.UserId = user.Id;
                userRole.RoleId = role.Id;
                userRole.CreatedAt = DateTime.UtcNow;

                await _userRepository.AddUserRoleAsync(userRole);
            }

            res.Message = "Register user succeed!";
            res.Data = _mapper.Map<UserResponse>(user);

            return res;
        }

        #endregion Register User

        public async Task<ResultResponse> Authenticate(AuthenRequest request)
        {
            var result = new ResultResponse<TokenResponseDto>();

            var hash = new PasswordHasher<UserEntity>();
            var user = await _userRepository.GetUserWithRolesAndPermissionsAsync(request.UserName);

            if (user == null || string.IsNullOrEmpty(user.Password))
            {
                return new ResultResponse(false, "Username/Password invalid!");
            }

            var verify = hash.VerifyHashedPassword(user, user.Password, request.Password);
            if (verify == PasswordVerificationResult.Failed)
            {
                return new ResultResponse(false, "Username/Password invalid!");
            }

            result.Success = true;
            result.Message = "Login successfully";
            result.Data = new TokenResponseDto
            {
                AccessToken = GenerateJWTToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user)
            };

            return result;
        }

        public async Task<ResultResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _userRepository.GetByUserIdAsync(request.UserId);

            if (user == null)
            {
                return new ResultResponse(false, "User not found!");
            }

            var refreshTokenEntity =
                await _tokenRepository.GetValidRefreshToken(
                    user.Id,
                    request.RefreshToken);

            if (refreshTokenEntity == null ||
                refreshTokenEntity.ExpirationTime < DateTime.UtcNow ||
                refreshTokenEntity.IsRevoke)
            {
                return new ResultResponse(
                    false,
                    "Invalid refresh token!");
            }

            // Generate refresh token mới
            var newRefreshToken = GenerateRefreshToken();

            // Rotation
            refreshTokenEntity.RefreshToken = newRefreshToken;
            refreshTokenEntity.ExpirationTime =
                DateTime.UtcNow.AddDays(7);

            await _tokenRepository.SaveChangesAsync();

            return new ResultResponse<TokenResponseDto>
            {
                Success = true,
                Message = "Refresh token successfully",
                Data = new TokenResponseDto
                {
                    AccessToken = GenerateJWTToken(user),
                    RefreshToken = newRefreshToken
                }
            };
        }

        #region Generate JWT Token

        private string GenerateJWTToken(UserEntity user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (user.UserRoles != null)
            {
                foreach (var userRole in user.UserRoles)
                {
                    if (userRole.Role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));

                        if (userRole.Role.RolePermissions != null)
                        {
                            foreach (var rolePermission in userRole.Role.RolePermissions)
                            {
                                if (rolePermission.Permission != null)
                                {
                                    claims.Add(new Claim("permission", rolePermission.Permission.Name));
                                }
                            }
                        }
                    }
                }
            }

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,

                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["Jwt:ExpireMinutes"]!)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion Generate JWT Token

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<string> GenerateAndSaveRefreshToken(UserEntity user)
        {
            var token = new RefreshTokenEntity();
            var refreshToken = GenerateRefreshToken();
            token.Id = Guid.NewGuid();
            token.UserId = user.Id;
            token.RefreshToken = refreshToken;
            token.ExpirationTime = DateTime.UtcNow.AddDays(7);

            await _tokenRepository.AddRefreshTokenAsync(token);
            return refreshToken;
        }

        public async Task<ResultResponse> Logout(RefreshTokenRequest request)
        {
            string prefixKeyBlackistJti = _configuration["RedisServer:PrefixKeyBlackListJti"] ?? "blacklist:jti:";
            var temp = _httpContext.HttpContext?.User;
            // 1. Lay jti va exp tu token
            string jti = temp?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var expClaim = temp?.FindFirst("exp")?.Value;

            TimeSpan? timeSpan = null;

            // 2. Parse exp sang long de tinh thoi gian con lai
            if (long.TryParse(expClaim, out var expUnixTime))
            {
                // Chuyen sang UTC time
                var expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;

                // Tu gio den exp con bao nhieu thoi gian
                var remainingTime = expirationDateTime - DateTime.UtcNow;

                if (remainingTime > TimeSpan.Zero)
                {
                    timeSpan = remainingTime;
                }
            }

            // 3. Đẩy jti vào Redis Blacklist với giá trị "revoked" và TTL chính là thời gian còn lại của token
            if (!string.IsNullOrEmpty(jti))
            {
                string redisKey = $"{prefixKeyBlackistJti}{jti}";
                await _redisService.SetAsync(redisKey, "revoked", expiry: timeSpan);
            }

            //string experied = _httpContext.HttpContext?.User.FindFirst()

            var refreshToken = await _tokenRepository
                .GetValidRefreshToken(
                    request.UserId,
                    request.RefreshToken);

            if (refreshToken != null)
            {
                refreshToken.IsRevoke = true;
                await _tokenRepository.SaveChangesAsync();
            }

            return new ResultResponse
            {
                Message = "Logout succeeded!"
            };
        }
    }
}