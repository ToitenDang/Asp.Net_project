namespace IdentityService.Models.Request
{
    public class PermissionRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}