using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.AuthenticationEvents
{
    internal sealed class OpenIdConnectEventsProxy(IAuthenticationEventsProvider<OpenIdConnectEvents> _originalEvents) : OpenIdConnectEvents
    {
        private OpenIdConnectEvents GetInner<T>(BaseContext<T> context)
            where T : RemoteAuthenticationOptions
        {
            return _originalEvents.GetOriginalEvents(context);
        }

        public override Task MessageReceived(MessageReceivedContext context)
        {
            return GetInner(context).MessageReceived(context);
        }

        public override Task RedirectToIdentityProvider(RedirectContext context)
        {
            return GetInner(context).RedirectToIdentityProvider(context);
        }

        public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
        {
            return GetInner(context).RedirectToIdentityProviderForSignOut(context);
        }   

        public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
        {
            return GetInner(context).AuthorizationCodeReceived(context);
        }

        public override Task TokenResponseReceived(TokenResponseReceivedContext context)
        {
            return GetInner(context).TokenResponseReceived(context);
        }

        public override Task TokenValidated(TokenValidatedContext context)
        {
            return InternalTokenValidatedAsync(context);
        }

        public override Task UserInformationReceived(UserInformationReceivedContext context)
        {
            return GetInner(context).UserInformationReceived(context);
        }

        public override Task TicketReceived(TicketReceivedContext context)
        {
            return GetInner(context).TicketReceived(context);
        }

        public override Task RemoteFailure(RemoteFailureContext context)
        {
            return GetInner(context).RemoteFailure(context);
        }

        private async Task InternalTokenValidatedAsync(TokenValidatedContext context)
        {
            await ExecuteBffTokenValidatedAsync(context).ConfigureAwait(false);
            await GetInner(context).TokenValidated(context).ConfigureAwait(false);
        }

        private async Task ExecuteBffTokenValidatedAsync(TokenValidatedContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.TokenEndpointResponse == null || string.IsNullOrWhiteSpace(context.TokenEndpointResponse.AccessToken))
                return;

            int expiresInSeconds = 0;
            if (!int.TryParse(context.TokenEndpointResponse.ExpiresIn, out expiresInSeconds))
                expiresInSeconds = 3600;

            var tokenReceivedService = context.HttpContext.RequestServices.GetRequiredService<ITokenReceivedService>();
            var tokenResponse = new TokenResponse
            {
                SessionId = Guid.NewGuid(),
                AccessToken = context.TokenEndpointResponse.AccessToken,
                RefreshToken = context.TokenEndpointResponse.RefreshToken,
                IdToken = context.TokenEndpointResponse.IdToken,
                ExpiresInSeconds = expiresInSeconds
            };

            context.Properties = context.Properties ?? new AuthenticationProperties();
            context.Properties.Items[CookieProperties.SessionId] = tokenResponse.SessionId.ToString();
            await tokenReceivedService.HandleAsync(tokenResponse).ConfigureAwait(false);
        }
    }
}
