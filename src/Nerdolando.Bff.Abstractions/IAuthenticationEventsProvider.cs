using Microsoft.AspNetCore.Authentication;

namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// Defines a mechanism for retrieving the original set of remote authentication events for a given authentication
    /// context.
    /// </summary>
    /// <remarks>Implementations of this interface allow customization of authentication event handling in
    /// remote authentication scenarios by providing access to the original event handlers based on the current
    /// authentication context. This is useful for scenarios where event processing needs to be extended or replaced
    /// dynamically.
    /// 
    /// Usually you will need to call original event in your proxy instead of your own work.
    /// 
    /// </remarks>
    /// <typeparam name="TEventType">The type of remote authentication events handled by this provider. Must derive from RemoteAuthenticationEvents
    /// and have a parameterless constructor.</typeparam>
    public interface IAuthenticationEventsProvider<TEventType> where TEventType : RemoteAuthenticationEvents, new()
    {
        /// <summary>
        /// Retrieves the original authentication events associated with the specified authentication context.
        /// </summary>
        /// <remarks>Use this method to access the initial set of authentication events configured for a
        /// given remote authentication context. This is useful when custom event handling or inspection is required
        /// during the authentication process.</remarks>
        /// <typeparam name="TOptions">The type of remote authentication options used by the context. Must derive from RemoteAuthenticationOptions.</typeparam>
        /// <param name="context">The authentication context that provides information required to obtain the original events. Cannot be null.</param>
        /// <returns>An instance of TEventType that represents the original authentication events for the provided context.</returns>
        /// <example>
        /// <code>
        /// var originalEvents = eventsProvider.GetOriginalEvents(context);
        /// originalEvents.AccessDenied(); // Call original AccessDenied event handler
        /// </code>
        /// </example>
        TEventType GetOriginalEvents<TOptions>(BaseContext<TOptions> context) where TOptions : RemoteAuthenticationOptions;
    }
}