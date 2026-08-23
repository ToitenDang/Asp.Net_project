namespace IdentityService.Models
{
    public class CurrentUser
    {
        public Guid? UserId { get; init; }
        public string? Email { get; init; }
        public string? UserName { get; init; }

        public IReadOnlyList<string> Roles { get; init; }
            = Array.Empty<string>();

        public IReadOnlyList<string> Permissions { get; init; }
            = Array.Empty<string>();
    }
}