using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Abstractions
{
    internal interface IUserInfoService
    {
        Task<IdentityDto?> GetCurrentUserIdentityAsync();
    }
}