using Microsoft.AspNetCore.Authentication.OAuth;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.AuthenticationEvents;

namespace Nerdolando.Bff.AspNetCore.Options
{
    internal sealed class OAuthFamilyConfigureHandler : BaseAuthOptionsConfigureHandler<OAuthOptions>
    {
        public OAuthFamilyConfigureHandler(IAuthenticationEventsRegistry _eventsRegistry)
            : base(_eventsRegistry)
        {

        }
        public override void Handle(OAuthOptions options, string schemeName)
        {
            RegisterBffEvents<OAuthEventsProxy, OAuthEvents>(options, schemeName);
        }
    }
}
