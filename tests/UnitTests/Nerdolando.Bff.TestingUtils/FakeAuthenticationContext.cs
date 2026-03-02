using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Nerdolando.Bff.TestingUtils
{
    public class FakeAuthenticationContext<TOptions> : BaseContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        public FakeAuthenticationContext(HttpContext context, AuthenticationScheme scheme, TOptions options) 
            : base(context, scheme, options)
        {
        }
    }
}
