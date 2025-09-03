using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nerdolando.Bff.Abstractions;
using System.Collections.Concurrent;

namespace Nerdolando.Bff.AspNetCore.Options;

/// <summary>
/// Post configuration logic for OpenIdConnectOptions used by the BFF.
/// Ensures a set of opinionated defaults suitable for BFF scenarios while
/// respecting values already explicitly set by the host application.
/// </summary>
internal sealed class BffOpenIdConnectPostConfigureOptions : IPostConfigureOptions<OpenIdConnectOptions>
{
    // Keeps original EventsType per scheme when we replace it with our composite.
    private static readonly ConcurrentDictionary<string, Type> OriginalEventsTypes = new(StringComparer.Ordinal);
    internal static Type? GetOriginalEventsType(string scheme)
        => OriginalEventsTypes.TryGetValue(scheme, out var t) ? t : null;
    public void PostConfigure(string? name, OpenIdConnectOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var scheme = name ?? OpenIdConnectDefaults.AuthenticationScheme;

        if (options.EventsType is not null)
        {
            // User chose EventsType. Wrap with composite (unless we already did).
            if (options.EventsType != typeof(BffCompositeOpenIdConnectEvents))
            {
                OriginalEventsTypes[scheme] = options.EventsType;
                options.EventsType = typeof(BffCompositeOpenIdConnectEvents);
            }
        }
        else
        {
            // User is using instance-based Events; wrap OnTokenValidated delegate.
            var existing = options.Events.OnTokenValidated;

            options.Events.OnTokenValidated = async context =>
            {
                await BffCompositeOpenIdConnectEvents.ExecuteBffTokenValidatedAsync(context).ConfigureAwait(false);

                if (existing is not null)
                    await existing(context).ConfigureAwait(false);
            };
        }
    }

    
}
