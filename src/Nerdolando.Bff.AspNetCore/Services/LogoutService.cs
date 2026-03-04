using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.AspNetCore.Abstractions;
using Nerdolando.Bff.AspNetCore.Models;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal sealed class LogoutService(IOptionsMonitor<BffConfig> _bffConfigMonitor,
        IAuthenticationSchemeProvider _authenticationSchemeProvider) : ILogoutService
    {
        public async Task<IResult> LogoutAsync(string front, string? returnUrl)
        {
            var bffConfig = _bffConfigMonitor.CurrentValue;
            var redirectUri = Utils.UriUtils.BuildRedirectUri(front, returnUrl, bffConfig);
            if (redirectUri == null)
                return Results.Problem("Invalid returnUrl.");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.RedirectUri = redirectUri.ToString();
            authenticationProperties.IsPersistent = true;

            var canSignOutFromChallenge = await CanSchemeSignOutAsync(bffConfig.Endpoints.ChallengeAuthenticationScheme).ConfigureAwait(false);
            string[] schemes = null!;
            if (canSignOutFromChallenge)
                schemes = [CookieAuthenticationDefaults.AuthenticationScheme, bffConfig.Endpoints.ChallengeAuthenticationScheme];
            else
                schemes = [CookieAuthenticationDefaults.AuthenticationScheme];

            return TypedResults.SignOut(authenticationProperties, schemes);
        }

        private async Task<bool> CanSchemeSignOutAsync(string schemeName)
        {
            var scheme = await _authenticationSchemeProvider.GetSchemeAsync(schemeName).ConfigureAwait(false);
            if (scheme == null)
                return false;

            return typeof(IAuthenticationSignOutHandler).IsAssignableFrom(scheme.HandlerType);
        }
    }
}
