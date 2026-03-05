using Nerdolando.Bff.Common;

namespace Nerdolando.Bff.AspNetCore.Abstractions
{
    internal interface IUserInfoService
    {
        Task<IdentityDto?> GetCurrentUserIdentityAsync();
    }
}