namespace IdentityService.Entities
{
    public class RefreshTokenEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
        public bool IsRevoke { get; set; } = false;
    }
}