using IdentityService.Models.OptionsPatternModels;

namespace IdentityService.Services.IService
{
    public interface ICurrentUserService
    {
        CurrentUser? User { get; }
    }
}