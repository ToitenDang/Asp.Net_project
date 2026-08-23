using IdentityService.Models;

namespace IdentityService.Services.IService
{
    public interface ICurrentUserService
    {
        CurrentUser? User { get; }
    }
}