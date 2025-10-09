using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal class AuthRefresher(ISessionIdProvider _sessionIdProvider,
        IUserTokenStorage _userTokenStorage,
        ITokenRefreshHandler _tokenRefreshHandler)
    {
        public async Task<UserToken?> GetOrRefreshAuthAsync()
        {
            Guid? guid = await _sessionIdProvider.GetSessionIdFromCurrentContextAsync().ConfigureAwait(false);
            if (guid == null)
                return new UserToken();

            UserToken? userToken = await _userTokenStorage.GetTokenAsync(guid.Value).ConfigureAwait(false);
            if (userToken == null)
                return new UserToken();

            if (userToken.ExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(5))
            {
                var newToken = await _tokenRefreshHandler.HandleAsync(userToken).ConfigureAwait(false);
                if (newToken != null)
                {
                    if (newToken.SessionId == Guid.Empty)
                        newToken.SessionId = Guid.NewGuid();

                    await _userTokenStorage.StoreTokenAsync(newToken).ConfigureAwait(false);
                    return newToken;
                }
                else
                    return null;
            }

            return userToken;
        }
    }
}
