using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal class DefaultTokenReceivedService(IUserTokenStorage _userTokenStorage) : ITokenReceivedService
    {
        public async Task HandleAsync(TokenResponse tokenResponse)
        {
            var userToken = new UserToken
            {
                SessionId = tokenResponse.SessionId,
                AccessToken = tokenResponse.AccessToken,
                RefreshToken = tokenResponse.RefreshToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresInSeconds)
            };

            await _userTokenStorage.StoreTokenAsync(userToken).ConfigureAwait(false);
        }
    }
}
