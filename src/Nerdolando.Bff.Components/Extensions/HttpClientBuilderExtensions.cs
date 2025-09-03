using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Components.Services;

namespace Nerdolando.Bff.Components.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class HttpClientBuilderExtensions
    {
        /// <summary>
        /// HttpClient adds credential cookie (HttpOnly) to outgoing requests.
        /// For this to work properly, target project must have CORS configured properly - it must allow credentials.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddCredentialCookie(this IHttpClientBuilder builder)
        {
            return builder.AddHttpMessageHandler<CredentialCookieDelegatingHandler>();
        }
    }
}
