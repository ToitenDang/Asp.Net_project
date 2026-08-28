using IdentityService.Authorization;
using IdentityService.Models.Request;
using IdentityService.Repositories.IRepository;
using IdentityService.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        //[Authorize(Roles = "ADMIN")]
        [HasPermission("GET_USERS")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _userService.GetAllUsers();
            return Ok(result);
        }

        //[HttpGet("{id}")]
        //[Authorize]
        //public Task<>

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromQuery] Guid userId, [FromBody] UserUpdateRequest request)
        {
            var result = await _userService.UpdateUserInfo(userId, request);
            return Ok(result);
        }
    }
}