using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Components.Abstractions;
using Nerdolando.Bff.Components.Models;

namespace Nerdolando.Bff.Components.Services
{
    internal class LogOutRequest(IHttpClientFactory _httpClientFactory,
        IAuthenticationStateRefresher _authStateRefresher,
        IOptionsMonitor<ClientBffOptions> _optionsMonitor, 
        NavigationManager _navManager) : ILogOutRequest
    {
        public async Task LogoutAsync(string returnUrl = "/")
        {
            var clientOptions = _optionsMonitor.CurrentValue;

            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = "/";

            var url = $"/auth{clientOptions.BffLogoutPath}?front={clientOptions.FrontAlias}&returnUrl={returnUrl}";

            if(!clientOptions.LogoutWithBackchannel)
                LogoutUsingFrontchannel(clientOptions, url);
            else
                await LogoutUsingBackchannel(url);
        }

        private async Task LogoutUsingBackchannel(string url)
        {
            using var httpClient = _httpClientFactory.CreateClient(BffDefaults.BffAuthenticationHttpClientName);
            var response = await httpClient.PostAsync(url, null);
            if (response.IsSuccessStatusCode)
            {
                await _authStateRefresher.RefreshAsync();
            }
        }

        private void LogoutUsingFrontchannel(ClientBffOptions bffOptions, string url)
        {
            var logoutUrl = new Uri(bffOptions.BffBaseAddress, url);
            _navManager.NavigateTo(logoutUrl.ToString(), forceLoad: true);
        }
    }
}
