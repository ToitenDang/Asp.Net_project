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

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser([FromRoute] Guid id)
        {
            var result = await _userService.GetUserById(id);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "PERSONAL_UPDATE")]
        public async Task<IActionResult> UpdateUserInfo([FromRoute] Guid id, [FromBody] UserUpdateRequest request)
        {
            var result = await _userService.UpdateUserInfo(id, request);
            return Ok(result);
        }
    }
}