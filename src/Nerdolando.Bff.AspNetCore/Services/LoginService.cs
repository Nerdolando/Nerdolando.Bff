using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.AspNetCore.Abstractions;
using Nerdolando.Bff.AspNetCore.Models;

namespace Nerdolando.Bff.AspNetCore.Services
{
    internal sealed class LoginService(IOptionsMonitor<BffConfig> _bffConfigMonitor): ILoginService
    {
        public IResult Login(string front, string? returnUrl)
        {
            var bffConfig = _bffConfigMonitor.CurrentValue;
            var redirectUri = Utils.UriUtils.BuildRedirectUri(front, returnUrl, bffConfig);
            if (redirectUri == null)
                return Results.BadRequest("Invalid returnUrl.");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.RedirectUri = redirectUri.ToString();
            authenticationProperties.IsPersistent = true;

            return TypedResults.Challenge(authenticationProperties, [bffConfig.Endpoints.ChallengeAuthenticationScheme]);
        }
    }
}
