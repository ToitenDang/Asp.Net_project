using AutoMapper;
using FluentValidation;
using IdentityService.Entities;
using IdentityService.Enum;
using IdentityService.Events;
using IdentityService.Models.OptionsPatternModels;
using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;
using IdentityService.Workers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IRedisService _redisService;
        private readonly EmailQueue _emailQueue;
        private readonly IMapper _mapper;
        private readonly IValidator<UserRequest> _validator;
        private readonly ITokenRepository _tokenRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        //private readonly EmailSettings _emailSetting;

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, IHttpContextAccessor httpContext, IRedisService redisService, EmailQueue emailQueue,
            IMapper mapper, ITokenRepository tokenRepository, IRoleRepository roleRepository, IUserRepository userRepository,
            IValidator<UserRequest> validator)
        {
            _configuration = configuration;
            _logger = logger;
            _httpContext = httpContext;
            _redisService = redisService;
            _emailQueue = emailQueue;
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

            await _emailQueue.QueueEmailAsync(new SendEmailEvent
            {
                ToEmail = request.Email,
                Subject = "Đăng ký tài khoản thành công.",
                Body = $"<h3>Bạn đã đăng ký thành công vào ứng dụng ....</p>"
            });

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
            // Cập nhật refreshToken revoke trong database
            var refreshToken = await _tokenRepository.GetValidRefreshToken(request.UserId, request.RefreshToken);

            if (refreshToken != null)
            {
                refreshToken.IsRevoke = true;
                await _tokenRepository.SaveChangesAsync();
            }

            try
            {
                var userClaims = _httpContext.HttpContext?.User;
                //  Lay jti va exp tu token
                string jti = userClaims?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                var expClaim = userClaims?.FindFirst("exp")?.Value;

                if (!string.IsNullOrEmpty(jti) && long.TryParse(expClaim, out var expUnixTime))
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(expUnixTime).UtcDateTime;
                    var remainingTime = expirationTime - DateTime.UtcNow;

                    // Chỉ lưu vào redis khi token còn hạn
                    if (remainingTime > TimeSpan.Zero)
                    {
                        string prefixKeyBlackistJti = _configuration["RedisServer:PrefixKeyBlackListJti"] ?? "blacklist:jti:";
                        string redisKey = $"{prefixKeyBlackistJti}{jti}";
                        await _redisService.SetAsync(redisKey, "revoked", expiry: remainingTime);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[Logout] Failed to blacklist JTI in Redis. User still logged out from DB.");
            }

            return new ResultResponse
            {
                Message = "Logout succeeded!"
            };
        }
    }
}