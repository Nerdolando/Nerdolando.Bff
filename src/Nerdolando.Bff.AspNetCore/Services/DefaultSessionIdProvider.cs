using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Nerdolando.Bff.AspNetCore.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal class DefaultSessionIdProvider(IHttpContextAccessor _httpContextAccessor) : ISessionIdProvider
    {
        public Task<Guid?> GetSessionIdFromCurrentContextAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            var authResultFeature = context?.Features.Get<IAuthenticateResultFeature>();
            var authProperties = authResultFeature?.AuthenticateResult?.Properties;

            if (authProperties == null)
                return Task.FromResult<Guid?>(null);


            if (authProperties.Items.TryGetValue(CookieProperties.SessionId, out var sessionIdFromAuth))
            {
                if (Guid.TryParse(sessionIdFromAuth, out var result))
                    return Task.FromResult<Guid?>(result);
            }

            return Task.FromResult<Guid?>(null);
        }
    }
}
