namespace IdentityService.Entities
{
    public class RoleEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<UserRoleEntity> UserRoles { get; set; }
        = new List<UserRoleEntity>();

        public ICollection<RolePermissionEntity> RolePermissions { get; set; }
            = new List<RolePermissionEntity>();
    }
}