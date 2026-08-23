namespace IdentityService.Entities
{
    public class RolePermissionEntity
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid PermissionId { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public RoleEntity Role { get; set; } = null!;
        public PermissionEntity Permission { get; set; } = null!;
    }
}