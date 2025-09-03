using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.Models;
using System.Net.Http.Headers;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal sealed class DefaultTokenRefreshHandler(IOptionsMonitor<OpenIdConnectOptions> _oidcOptionsMonitor, 
        IOptions<BffConfig> _bffConfig) : ITokenRefreshHandler
    {
        public async Task<UserToken?> HandleAsync(UserToken userToken)
        {
            var oidcOptions = GetOIDCOptions();
            EnsureValidOptions(oidcOptions);

            OpenIdConnectMessage? refreshTokenMessage = await SendRefreshTokenRequestAsync(oidcOptions, userToken.RefreshToken!).ConfigureAwait(false);
            if(refreshTokenMessage != null) 
            {
                int expiresIn = 3600;
                if(int.TryParse(refreshTokenMessage.ExpiresIn, out var parsedValue))
                    expiresIn = parsedValue;

                return new UserToken
                {
                    AccessToken = refreshTokenMessage.AccessToken,
                    RefreshToken = refreshTokenMessage.RefreshToken,
                    ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresIn),
                    SessionId = userToken.SessionId
                };
            }
            else
                return null;
        }

        private OpenIdConnectOptions GetOIDCOptions()
        {
            var candidate = _oidcOptionsMonitor.CurrentValue;
            if(string.IsNullOrWhiteSpace(candidate.ClientId) || string.IsNullOrWhiteSpace(candidate.ClientSecret))
               return _oidcOptionsMonitor.Get(_bffConfig.Value.Endpoints.ChallengeAuthenticationScheme);
            else
                return candidate;
        }

        private static void EnsureValidOptions(OpenIdConnectOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.ClientId))
                throw new InvalidOperationException("ClientId is not configured in OpenIdConnectOptions.");
            if (string.IsNullOrWhiteSpace(options.ClientSecret))
                throw new InvalidOperationException("ClientSecret is not configured in OpenIdConnectOptions.");
        }

        private async Task<string> GetTokenEndpointAsync(OpenIdConnectOptions options)
        {
            Uri? candidate = _bffConfig.Value.Endpoints.RefreshTokenEndpoint;
            if (candidate != null)
                return candidate.ToString();

            if(options.ConfigurationManager == null)
                throw new InvalidOperationException("ConfigurationManager is not configured in OpenIdConnectOptions. Either OpenIdConnectOptions.ConfigurationManager or BffConfig.Endpoints.RefreshTokenEndpoint must be configured");

            var oidcConfig = await options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None).ConfigureAwait(false);
            if(oidcConfig == null)
                throw new InvalidOperationException("Unable to retrieve OpenIdConnect configuration. Ensure that metadata endpoint is valid or simply configure BffConfig.Endpoints.RefreshTokenEndpoint");

            if (string.IsNullOrWhiteSpace(oidcConfig.TokenEndpoint))
                throw new InvalidOperationException("TokenEndpoint is not configured in OpenIdConnect configuration.");
            return oidcConfig.TokenEndpoint;
        }

        private async Task<OpenIdConnectMessage?> SendRefreshTokenRequestAsync(OpenIdConnectOptions oidcOptions, string refreshToken)
        {
            var values = new Dictionary<string, string>
            {
                {"grant_type", "refresh_token" },
                {"client_id", oidcOptions.ClientId! },
                {"client_secret", oidcOptions.ClientSecret! },
                {"refresh_token", refreshToken }
            };

            var content = new FormUrlEncodedContent(values);
            var tokenEndpoint = await GetTokenEndpointAsync(oidcOptions).ConfigureAwait(false);

            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint);
            request.Content = content;
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await oidcOptions.Backchannel.SendAsync(request, CancellationToken.None).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new OpenIdConnectMessage(dto);
            }

            return null;
        }
    }
}
