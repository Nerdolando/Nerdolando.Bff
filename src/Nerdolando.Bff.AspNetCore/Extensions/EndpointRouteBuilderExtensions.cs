using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.Models;
using Nerdolando.Bff.AspNetCore.Services;
using System.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

namespace Nerdolando.Bff.AspNetCore.Extensions
{
    /// <summary>
    /// Bff extensions for <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static class EndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Maps the Bff endpoints. Must be called after authentication and authorization middlewares.
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapBff(this IEndpointRouteBuilder endpoints)
        {
            var bffOptionsMonitor = endpoints.ServiceProvider.GetRequiredService<IOptionsMonitor<BffConfig>>();
            var bffConfig = bffOptionsMonitor.CurrentValue;
            var apiGroup = endpoints.MapGroup(bffConfig.Endpoints.TargetApiPath);

            apiGroup.MapForwarder("{**catch-all}", bffConfig.Endpoints.TargetApiBaseUrl.ToString(), transformBuilder =>
            {
                transformBuilder.AddRequestTransform(async transformContext =>
                {
                    var refresher = transformContext.HttpContext.RequestServices.GetRequiredService<AuthRefresher>();
                    var userToken = await refresher.GetOrRefreshAuthAsync().ConfigureAwait(false);
                    if (userToken == null)
                    {
                        transformContext.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userToken.AccessToken);
                });
            }).RequireAuthorization();

            return endpoints.MapAuthEndpoints(bffConfig.Endpoints);
        }
        internal static IEndpointConventionBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints, BffEndpointConfig config)
        {
            var group = endpoints.MapGroup("/auth");

            group.MapGet(config.BffLoginPath, (string front, string? returnUtl, HttpContext httpContext) => Login(front, returnUtl, httpContext, config))
                .AllowAnonymous();

            group.MapPost(config.BffLogoutPath, (string front, string? returnUrl, HttpContext httpContext) => Logout(front, returnUrl, httpContext, config));

            group.MapGet(config.BffUserInfoPath, async (HttpContext httpContext) =>
            {
                var refresher = httpContext.RequestServices.GetRequiredService<AuthRefresher>();
                var userToken = await refresher.GetOrRefreshAuthAsync().ConfigureAwait(false);

                if (userToken == null)
                {
                    return Results.Unauthorized();
                }

                var identityDto = new IdentityDto
                {
                    AuthenticationType = httpContext.User.Identity?.AuthenticationType ?? string.Empty,
                    Claims = httpContext.User.Claims.Select(c => new IdentityClaim { Type = c.Type, Value = c.Value })
                };

                return Results.Ok(identityDto);

            }).RequireAuthorization();

            return group;
        }

        internal static IResult Login(string front,
            string? returnUrl,
            HttpContext httpContext,
            BffEndpointConfig config)
        {
            var redirectUri = BuildRedirectUri(front, returnUrl ?? "/", httpContext);
            if (redirectUri == null)
                return Results.BadRequest("Invalid returnUrl");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.RedirectUri = redirectUri.ToString();
            authenticationProperties.IsPersistent = true;

            return TypedResults.Challenge(authenticationProperties, [config.ChallengeAuthenticationScheme]);
        }

        internal static IResult Logout(string front,
            string? returnUrl,
            HttpContext httpContext,
            BffEndpointConfig config)
        {
            var redirectUri = BuildRedirectUri(front, returnUrl ?? "/", httpContext);
            if (redirectUri == null)
                return Results.Problem("Invalid returnUrl");

            var authenticationProperties = new AuthenticationProperties();
            authenticationProperties.RedirectUri = redirectUri.ToString();
            authenticationProperties.IsPersistent = true;

            return TypedResults.SignOut(authenticationProperties, [CookieAuthenticationDefaults.AuthenticationScheme, config.ChallengeAuthenticationScheme]);
        }

        internal static Uri? BuildRedirectUri(string frontType, string? returnUrl, HttpContext httpContext)
        {
            var bffOptionsMonitor = httpContext.RequestServices.GetRequiredService<IOptionsMonitor<BffConfig>>();
            var bffOptions = bffOptionsMonitor.CurrentValue;

            if (!bffOptions.FrontUrls.TryGetValue(frontType, out Uri? value))
                return null;

            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = "/";

            if (returnUrl.IndexOfAny(['\\', '\r', '\n']) >= 0)
                return null;

            var ub = new UriBuilder(value!);
            ub.Path = returnUrl;
            return ub.Uri;
        }
    }
}
