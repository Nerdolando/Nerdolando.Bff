using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.Components.Abstractions;
using Nerdolando.Bff.Components.Models;
using System.Net.Http.Json;
using System.Security.Claims;

namespace Nerdolando.Bff.Components.Services
{
    internal class BffAuthenticationStateProvider(IHttpClientFactory _httpClientFactory, 
        IOptionsMonitor<ClientBffOptions> _optionsMonitor) : AuthenticationStateProvider, IAuthenticationStateRefresher
    {
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var options = _optionsMonitor.CurrentValue;
            var meUrl = $"/auth{options.BffUserInfoPath}";

            using var httpClient = _httpClientFactory.CreateClient(BffDefaults.BffAuthenticationHttpClientName);
            using var response = await httpClient.GetAsync(meUrl).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                var identityDto = await response.Content.ReadFromJsonAsync<IdentityDto>();
                if (identityDto != null)
                {
                    var claims = identityDto.Claims.Select(c => new Claim(c.Type, c.Value));
                    var identity = new ClaimsIdentity(claims, identityDto.AuthenticationType);
                    var user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }
            }
            return new AuthenticationState(new ClaimsPrincipal());
        }

        public async Task RefreshAsync()
        {
            var state = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
    }
}
