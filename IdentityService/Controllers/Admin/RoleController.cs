using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<ActionResult<ResultResponse>> CreatePermission(RoleRequest request)
        {
            var res = await _roleService.CreateRole(request);
            return res;
        }

        [HttpPut("id")]
        public async Task<ActionResult<ResultResponse>> UpdatePermission([FromRoute] Guid Id, [FromBody] RoleRequest request)
        {
            var res = await _roleService.UpdateRole(Id, request);
            return Ok(res);
        }

        [HttpGet]
        public async Task<ActionResult<ResultResponse>> GetAll()
        {
            var res = await _roleService.GetAllRoles();
            return res;
        }

        [HttpGet("id")]
        public async Task<ActionResult<ResultResponse>> GetRoleById([FromRoute] Guid Id)
        {
            var res = await _roleService.GetById(Id);
            return res;
        }
    }
}