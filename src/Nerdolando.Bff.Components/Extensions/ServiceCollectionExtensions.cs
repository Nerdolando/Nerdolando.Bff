using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Components.Abstractions;
using Nerdolando.Bff.Components.Models;
using Nerdolando.Bff.Components.Services;

namespace Nerdolando.Bff.Components.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds BFF services with default configuration.
        /// </summary>
        /// <param name="services">The service collection to add the BFF services to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddBffServices(this IServiceCollection services)
        {
            return AddBffServices(services, _ => { });
        }

        /// <summary>
        /// Adds BFF services with custom configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="options">Action to configure options</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IServiceCollection AddBffServices(this IServiceCollection services, Action<ClientBffOptions> options)
        {
            services.AddOptions<ClientBffOptions>()
                .Configure(options)
                .ValidateOnStart();

            services.AddHttpClient(BffDefaults.BffAuthenticationHttpClientName, (sp, client) =>
            {
                var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<ClientBffOptions>>();
                var options = optionsMonitor.CurrentValue;
                client.BaseAddress = options.BffBaseAddress;
            }).AddCredentialCookie();

            services.AddTransient<CredentialCookieDelegatingHandler>();
            services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();
            services.AddTransient<ILogOutRequest, LogOutRequest>();

            services.AddScoped(sp =>
            {
                var provider = sp.GetRequiredService<AuthenticationStateProvider>();
                if (provider is IAuthenticationStateRefresher refresher)
                    return refresher;

                throw new InvalidOperationException("The registered AuthenticationStateProvider does not implement IAuthenticationStateRefresher.");
            });

            return services;
        }   
    }
}
