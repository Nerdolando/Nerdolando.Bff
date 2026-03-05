using Microsoft.AspNetCore.Authentication;

namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// Provides a base class for configuring authentication configuration handler for a specific authentication scheme. Derived
    /// classes implement custom logic to handle and configure options of the specified type.
    /// </summary>
    /// <remarks>This abstract class is intended to be extended by implementers who need to provide custom
    /// configuration for authentication options. It offers a mechanism to determine if a handler can process a given
    /// options type and provides support for registering authentication events. Implementers must override the Handle
    /// method to define specific configuration logic for the options.
    /// In most cases calling <code>RegisterBffEvents</code> method inside <code>Handle</code> method should be enough.
    /// You should rather derive from this class instead of implementing IAuthOptionsConfigureHandler/<TOptions/> directly, because it provides some useful helper methods and default implementation for CanHandle method.
    /// </remarks>
    /// <typeparam name="TOptions">The type of authentication scheme options to configure. Must inherit from AuthenticationSchemeOptions.</typeparam>
    /// <param name="_eventsRegistry">The registry used to manage and register authentication events for the associated authentication scheme.</param>
    public abstract class BaseAuthOptionsConfigureHandler<TOptions>(IAuthenticationEventsRegistry _eventsRegistry) : IAuthOptionsConfigureHandler<TOptions>
        where TOptions : AuthenticationSchemeOptions
    {

        /// <summary>
        /// Determines whether the specified options type is compatible with the current handler instance.
        /// Called by library code to find the appropriate handler for a given options type. The default implementation checks if the provided options type can be assigned to the handler's generic type parameter <typeparamref name="TOptions"/>.
        /// </summary>
        /// <remarks>Use this method to verify that a given options type is supported by the handler
        /// before attempting to configure or use it.</remarks>
        /// <param name="optionsType">The type of options to check for compatibility. This parameter cannot be null.</param>
        /// <returns>true if the specified options type can be assigned to the handler's options type; otherwise, false.</returns>
        public virtual bool CanHandle(Type optionsType)
        {
            ArgumentNullException.ThrowIfNull(optionsType);
            return typeof(TOptions).IsAssignableFrom(optionsType);
        }

        /// <summary>
        /// Handles the specified options using the provided authentication scheme name.
        /// </summary>
        /// <remarks>Override this method in a derived class to implement custom logic for handling
        /// options based on the specified scheme name.
        /// Especially you should register your own OptionsEventType Proxy using the RegisterBffEvents method, so that you can handle the events in your own way and still allow users to use their own events implementation if they want to.
        /// </remarks>
        /// <param name="options">The options to be processed. Must be a valid instance of the options type expected by the handler.</param>
        /// <param name="schemeName">The name of the authentication scheme that determines the context for processing the options.</param>
        public abstract void Handle(TOptions options, string schemeName);

        /// <summary>
        /// Registers the specified BFF (Backend for Frontend) events proxy for the given authentication scheme.
        /// </summary>
        /// <remarks>This method ensures that the correct event type is set for the specified
        /// authentication scheme and updates the event registry accordingly. If no event type is specified, the
        /// original event type is used by default.</remarks>
        /// <typeparam name="TBffEventType">The type of BFF event to register. Must derive from the base event type.</typeparam>
        /// <typeparam name="TOriginalEventType">The original event type to use if no BFF event type is provided. Must inherit from
        /// RemoteAuthenticationEvents and have a parameterless constructor.</typeparam>
        /// <param name="options">The options object containing configuration settings for the authentication scheme and events.</param>
        /// <param name="schemeName">The name of the authentication scheme for which the BFF events are being registered. Cannot be null or
        /// whitespace.</param>
        protected void RegisterBffEvents<TBffEventType, TOriginalEventType>(TOptions options, string schemeName)
            where TOriginalEventType : RemoteAuthenticationEvents, new()
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(schemeName);

            if (options.EventsType != null)
            {
                if (options.EventsType != typeof(TBffEventType))
                {
                    _eventsRegistry.Set(options.GetType(), schemeName, options.EventsType);
                    options.EventsType = typeof(TBffEventType);
                }
            }
            else
            {
                options.Events ??= new TOriginalEventType();
                _eventsRegistry.Set(options.GetType(), schemeName, options.Events);
            }

            options.Events = null!;
            options.EventsType = typeof(TBffEventType);
        }
    }
}
