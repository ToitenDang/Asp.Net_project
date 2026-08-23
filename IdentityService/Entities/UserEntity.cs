namespace IdentityService.Entities
{
    public class UserEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<UserRoleEntity> UserRoles { get; set; }
        = new List<UserRoleEntity>();
    }
}