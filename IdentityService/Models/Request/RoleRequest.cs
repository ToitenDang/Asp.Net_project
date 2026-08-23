using System;
using System.Collections.Generic;

namespace IdentityService.Models.Request
{
    public class RoleRequest
    {
        public string? Name { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Guid> PermissionIds { get; set; } = new List<Guid>();
    }
}