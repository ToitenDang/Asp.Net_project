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
    [Authorize(Roles = "ADMIN")]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission(PermissionRequest request)
        {
            var res = await _permissionService.CreatePermission(request);
            return Ok(res);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission([FromRoute] Guid Id, [FromBody] PermissionRequest request)
        {
            var res = await _permissionService.UpdatePermission(Id, request);
            return Ok(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var res = await _permissionService.GetAllPermissions();
            return Ok(res);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById([FromRoute] Guid Id)
        {
            var res = await _permissionService.GetPermissionById(Id);
            return Ok(res);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermissionById([FromRoute] Guid Id)
        {
            var res = await _permissionService.DeletePermisson(Id);
            return Ok(res);
        }
    }
}