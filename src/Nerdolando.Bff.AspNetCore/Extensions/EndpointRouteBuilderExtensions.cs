using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.AspNetCore.Abstractions;
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

                    var requiredAccessToken = bffConfig.UseIdTokenAsAccessToken && !string.IsNullOrWhiteSpace(userToken.IdToken) ? userToken.IdToken : userToken.AccessToken;

                    transformContext.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requiredAccessToken);
                });
            }).RequireAuthorization();

            return endpoints.MapAuthEndpoints(bffConfig.Endpoints);
        }
        internal static IEndpointConventionBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints, BffEndpointConfig config)
        {
            var group = endpoints.MapGroup("/auth");

            group.MapGet(config.BffLoginPath, (string front, string? returnUtl, ILoginService loginService) =>
            {
                return loginService.Login(front, returnUtl);
            })
            .AllowAnonymous();

            group.MapPost(config.BffLogoutPath, async (string front, string? returnUrl, ILogoutService logoutService) =>
            {
                return await logoutService.LogoutAsync(front, returnUrl).ConfigureAwait(false);
            });

            group.MapGet(config.BffUserInfoPath, async (IUserInfoService userInfoService) =>
            {
                var userInfo = await userInfoService.GetCurrentUserIdentityAsync().ConfigureAwait(false);

                if (userInfo == null)
                    return Results.Unauthorized();

                return Results.Ok(userInfo);

            }).RequireAuthorization();

            return group;
        }        
    }
}
