namespace IdentityService.Models.Response
{
    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
    }
}