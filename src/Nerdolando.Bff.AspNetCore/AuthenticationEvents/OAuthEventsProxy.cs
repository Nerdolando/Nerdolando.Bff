using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.Common;

namespace Nerdolando.Bff.AspNetCore.AuthenticationEvents
{
    /// <summary>
    /// Decorator that wraps the original user-supplied OAuth (via EventsType)
    /// while injecting BFF logic into TokenValidated.
    /// </summary>
    internal sealed class OAuthEventsProxy(IAuthenticationEventsProvider<OAuthEvents> _originalEventsProvider) : OAuthEvents
    {
        private OAuthEvents GetInner<T>(BaseContext<T> context)
            where T : RemoteAuthenticationOptions
        {
            return _originalEventsProvider.GetOriginalEvents(context);
        }

        public override Task AccessDenied(AccessDeniedContext context)
        {
            return GetInner(context).AccessDenied(context);
        }

        public override Task CreatingTicket(OAuthCreatingTicketContext context)
        {
            return InternalCreatingTicketAsync(context);
        }

        public override Task RedirectToAuthorizationEndpoint(RedirectContext<OAuthOptions> context)
        {
            return GetInner(context).RedirectToAuthorizationEndpoint(context);
        }

        public override Task RemoteFailure(RemoteFailureContext context)
        {
            return GetInner(context).RemoteFailure(context);
        }

        public override Task TicketReceived(TicketReceivedContext context)
        {
            return GetInner(context).TicketReceived(context);
        }

        private async Task InternalCreatingTicketAsync(OAuthCreatingTicketContext context)
        {
            await ExecuteBffCreatingTicketAsync(context).ConfigureAwait(false);
            await GetInner(context).CreatingTicket(context).ConfigureAwait(false);
        }

        private async Task ExecuteBffCreatingTicketAsync(OAuthCreatingTicketContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (string.IsNullOrWhiteSpace(context.AccessToken))
                return;

            int expiresInSeconds = 0;
            if (!context.ExpiresIn.HasValue)
                expiresInSeconds = 3600;
            else
                expiresInSeconds = (int)context.ExpiresIn.Value.TotalSeconds;

            var idToken = context.TokenResponse.Response!.RootElement.TryGetProperty("id_token", out var idTokenProperty) ? idTokenProperty.GetString() : null;
            var tokenResponse = new TokenResponse
            {
                SessionId = Guid.NewGuid(),
                AccessToken = context.AccessToken!,
                RefreshToken = context.RefreshToken,
                ExpiresInSeconds = expiresInSeconds,
                IdToken = idToken
            };

            context.Properties = context.Properties ?? new AuthenticationProperties();
            context.Properties.Items[CookieProperties.SessionId] = tokenResponse.SessionId.ToString();

            var tokenReceivedService = context.HttpContext.RequestServices.GetRequiredService<ITokenReceivedService>();
            await tokenReceivedService.HandleAsync(tokenResponse).ConfigureAwait(false);
        }
    }
}
