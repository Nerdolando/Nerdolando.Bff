using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.Options
{
    /// <summary>
    /// Decorator that wraps the original user-supplied OpenIdConnectEvents (via EventsType)
    /// while injecting BFF logic into TokenValidated.
    /// </summary>
    internal sealed class BffCompositeOpenIdConnectEvents : OpenIdConnectEvents
    {
        // Lazily created per request (scoped provider)
        private OpenIdConnectEvents? _inner;

        private OpenIdConnectEvents GetInner<T>(BaseContext<T> context)
            where T : RemoteAuthenticationOptions
        {
            if (_inner != null) 
                return _inner;

            var scheme = context.Scheme.Name;
            var originalType = BffOpenIdConnectPostConfigureOptions.GetOriginalEventsType(scheme) ?? typeof(OpenIdConnectEvents);

            // Use the current request scope so scoped services in the original events type work.
            var sp = context.HttpContext.RequestServices;
            _inner = (OpenIdConnectEvents)ActivatorUtilities.CreateInstance(sp, originalType);
            return _inner;
        }

        // ---- Overridden Event Methods ----
        // We forward all to inner except we inject logic in TokenValidated.

        public override Task MessageReceived(MessageReceivedContext context)
            => GetInner(context).MessageReceived(context);

        public override Task RedirectToIdentityProvider(RedirectContext context)
            => GetInner(context).RedirectToIdentityProvider(context);

        public override Task RedirectToIdentityProviderForSignOut(RedirectContext context)
            => GetInner(context).RedirectToIdentityProviderForSignOut(context);

        public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context)
            => GetInner(context).AuthorizationCodeReceived(context);

        public override Task TokenResponseReceived(TokenResponseReceivedContext context)
            => GetInner(context).TokenResponseReceived(context);

        public override Task TokenValidated(TokenValidatedContext context)
            => ExecuteBffThenInnerAsync(context);

        public override Task UserInformationReceived(UserInformationReceivedContext context)
            => GetInner(context).UserInformationReceived(context);

        public override Task TicketReceived(TicketReceivedContext context)
            => GetInner(context).TicketReceived(context);

        public override Task RemoteFailure(RemoteFailureContext context)
            => GetInner(context).RemoteFailure(context);

        private async Task ExecuteBffThenInnerAsync(TokenValidatedContext context)
        {
            // Our BFF logic (same as delegate path)
            await ExecuteBffTokenValidatedAsync(context).ConfigureAwait(false);

            // Then user logic (inner).
            await GetInner(context).TokenValidated(context).ConfigureAwait(false);
        }

        public static async Task ExecuteBffTokenValidatedAsync(TokenValidatedContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            
            if(context.TokenEndpointResponse == null)
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
                ExpiresInSeconds = expiresInSeconds
            };

            context.Properties = context.Properties ?? new AuthenticationProperties();
            context.Properties.Items[CookieProperties.SessionId] = tokenResponse.SessionId.ToString();
            await tokenReceivedService.HandleAsync(tokenResponse).ConfigureAwait(false);
        }
    }
}
