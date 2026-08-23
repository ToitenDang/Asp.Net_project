namespace IdentityService.Entities
{
    public class UserRoleEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        public UserEntity User { get; set; } = null!;
        public RoleEntity Role { get; set; } = null!;
    }
}