using IdentityService.Models.Request;
using IdentityService.Models.Response;
using IdentityService.Services.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IdentityService.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpPost]
        public async Task<ActionResult<ResultResponse>> CreatePermission(PermissionRequest request)
        {
            var res = await _permissionService.CreatePermission(request);
            return res;
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResultResponse>> UpdatePermission([FromRoute] Guid Id, [FromBody] PermissionRequest request)
        {
            var res = await _permissionService.UpdatePermission(Id, request);
            return Ok(res);
        }

        [HttpGet]
        public async Task<ActionResult<ResultResponse>> GetAll()
        {
            var res = await _permissionService.GetAllPermissions();
            return res;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResultResponse>> GetPermissionById([FromRoute] Guid Id)
        {
            var res = await _permissionService.GetPermissionById(Id);
            return res;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResultResponse>> DeletePermissionById([FromRoute] Guid Id)
        {
            var res = await _permissionService.DeletePermisson(Id);
            return res;
        }
    }
}