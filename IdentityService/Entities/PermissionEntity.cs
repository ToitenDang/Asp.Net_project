namespace IdentityService.Entities
{
    public class PermissionEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string PermissionCode { set; get; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<RolePermissionEntity> RolePermissions { get; set; }
        = new List<RolePermissionEntity>();
    }
}