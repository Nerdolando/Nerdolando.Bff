using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Nerdolando.Bff.Abstractions;

namespace Nerdolando.Bff.AspNetCore.AuthenticationEvents
{
    internal sealed class AuthenticationEventsProvider<TEventType>(IAuthenticationEventsRegistry _eventsRegistry) : IAuthenticationEventsProvider<TEventType> where TEventType : RemoteAuthenticationEvents, new()
    {
        private TEventType? _cachedEvents;

        public TEventType GetOriginalEvents<TOptions>(BaseContext<TOptions> context)
            where TOptions : RemoteAuthenticationOptions
        {
            if (_cachedEvents != null)
                return _cachedEvents;

            var scheme = context.Scheme.Name;

            if (!_eventsRegistry.TryGet(context.Options.GetType(), scheme, out var original))
                return new TEventType();

            var sp = context.HttpContext.RequestServices;
            TEventType inner = original switch
            {
                Type originalType => (TEventType)ActivatorUtilities.CreateInstance(sp, originalType),
                TEventType originalInstance => originalInstance,
                _ => new TEventType()
            };

            _cachedEvents = inner;
            return _cachedEvents;
        }
    }
}
