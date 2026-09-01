using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission(RoleRequest request)
        {
            var res = await _roleService.CreateRole(request);
            return Ok(res);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdatePermission([FromRoute] Guid Id, [FromBody] RoleRequest request)
        {
            var res = await _roleService.UpdateRole(Id, request);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await _roleService.GetAllRoles();
            return Ok(res);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetRoleById([FromRoute] Guid Id)
        {
            var res = await _roleService.GetById(Id);
            return Ok(res);
        }
    }
}