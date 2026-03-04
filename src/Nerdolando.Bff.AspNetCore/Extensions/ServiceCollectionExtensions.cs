using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Abstractions;
using Nerdolando.Bff.AspNetCore.Abstractions;
using Nerdolando.Bff.AspNetCore.AuthenticationEvents;
using Nerdolando.Bff.AspNetCore.Models;
using Nerdolando.Bff.AspNetCore.Options;
using Nerdolando.Bff.AspNetCore.Services;

namespace Nerdolando.Bff.AspNetCore.Extensions
{
    /// <summary>
    /// Bff extensions for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {

        /// <summary>
        /// Adds the BFF services to the <see cref="IServiceCollection"/> with default configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IBffBuilder AddBff(this IServiceCollection services)
        {
            return AddBff(services, _ => { });
        }

        /// <summary>
        /// Adds the BFF services to the <see cref="IServiceCollection"/> with custom configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IBffBuilder AddBff(this IServiceCollection services, Action<BffConfig> configure)
        {
            var builder = new BffBuilder(services);

            services.Configure(configure);

            services.AddHttpForwarder();

            services.AddScoped<ITokenReceivedService, DefaultTokenReceivedService>();
            services.AddScoped<ITokenRefreshHandler, DefaultTokenRefreshHandler>();
            services.AddScoped<AuthRefresher>();
            services.AddScoped<ISessionIdProvider, DefaultSessionIdProvider>();

            services.AddHttpContextAccessor();

            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<ILogoutService, LogoutService>();
            services.AddScoped<IUserInfoService, UserInfoService>();

            services.AddTransient<OAuthEventsProxy>();
            services.AddTransient<OpenIdConnectEventsProxy>();

            services.AddSingleton<IAuthenticationEventsRegistry, AuthenticationEventsRegistry>();
            
            services.TryAdd(ServiceDescriptor.Transient(typeof(IAuthenticationEventsProvider<>),
                typeof(AuthenticationEventsProvider<>)));
            
            services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IPostConfigureOptions<>), 
                typeof(BffPostConfigureOptions<>)));

            services.AddSingleton<IAuthOptionsConfigureHandler<OAuthOptions>, OAuthFamilyConfigureHandler>();
            services.AddSingleton<IAuthOptionsConfigureHandler<OpenIdConnectOptions>, OidcFamilyConfigureHandler>();
            services.AddSingleton<IConfigureHandlerProvider, ConfigureHandlerProvider>();
            return builder;
        }
    }
}
