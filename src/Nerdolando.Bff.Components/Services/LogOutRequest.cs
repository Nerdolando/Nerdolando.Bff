using Microsoft.Extensions.Options;
using Nerdolando.Bff.Components.Abstractions;
using Nerdolando.Bff.Components.Models;

namespace Nerdolando.Bff.Components.Services
{
    internal class LogOutRequest(IHttpClientFactory _httpClientFactory,
        IAuthenticationStateRefresher _authStateRefresher,
        IOptionsMonitor<ClientBffOptions> _optionsMonitor) : ILogOutRequest
    {
        public async Task LogoutAsync(string returnUrl = "/")
        {
            using var httpClient = _httpClientFactory.CreateClient(BffDefaults.BffAuthenticationHttpClientName);
            var clientOptions = _optionsMonitor.CurrentValue;

            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = "/";

            var url = $"/auth{clientOptions.BffLogoutPath}?front={clientOptions.FrontAlias}&returnUrl={returnUrl}";

            var response = await httpClient.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                await _authStateRefresher.RefreshAsync();
            }
        }
    }
}
