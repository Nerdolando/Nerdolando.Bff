using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.AuthenticationEvents;

namespace Nerdolando.Bff.AspNetCore.Options
{
    internal sealed class OidcFamilyConfigureHandler : BaseAuthOptionsConfigureHandler<OpenIdConnectOptions>
    {
        public OidcFamilyConfigureHandler(IAuthenticationEventsRegistry _eventsRegistry)
            :base(_eventsRegistry)
        {
            
        }
        public override void Handle(OpenIdConnectOptions options, string schemeName)
        {
            RegisterBffEvents<OpenIdConnectEventsProxy, OpenIdConnectEvents>(options, schemeName);
        }
    }
}
