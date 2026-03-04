using Microsoft.AspNetCore.Http;
using Nerdolando.Bff.AspNetCore.Abstractions;
using Nerdolando.Bff.Common;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal class UserInfoService(IHttpContextAccessor _httpContextAccessor,
        AuthRefresher _authRefresher) : IUserInfoService
    {
        public async Task<IdentityDto?> GetCurrentUserIdentityAsync()
        {
            var userToken = await _authRefresher.GetOrRefreshAuthAsync().ConfigureAwait(false);
            if (userToken == null)
                return null;

            var ctx = _httpContextAccessor.HttpContext;
            ArgumentNullException.ThrowIfNull(ctx);

            return new IdentityDto
            {
                AuthenticationType = ctx.User.Identity?.AuthenticationType ?? string.Empty,
                Claims = ctx.User.Claims.Select(c => new IdentityClaim
                {
                    Type = c.Type,
                    Value = c.Value
                })
            };
        }
    }
}
