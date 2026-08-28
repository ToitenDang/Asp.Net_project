using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ResultResponse>> Login([FromBody] AuthenRequest request)
        {
            var result = await _authService.Authenticate(request);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult<ResultResponse>> Logout(RefreshTokenRequest request)
        {
            var result = await _authService.Logout(request);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<ResultResponse>> Register([FromBody] UserRequest request)
        {
            var result = await _authService.Registered(request);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ResultResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(result);
        }
    }
}