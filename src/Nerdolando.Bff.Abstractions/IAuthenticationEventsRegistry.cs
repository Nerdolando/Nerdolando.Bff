namespace Nerdolando.Bff.Abstractions
{

    /// <summary>
    /// Defines methods for registering and retrieving original authentication event handlers associated with specific
    /// authentication schemes and options types.
    /// </summary>
    /// <remarks>Implementations of this interface should ensure thread safety when accessing or modifying the
    /// registry. This interface enables dynamic customization of authentication behavior by allowing event handlers to
    /// be set or retrieved at runtime based on the authentication scheme and options type.</remarks>
    public interface IAuthenticationEventsRegistry
    {
        /// <summary>
        /// Sets the authentication scheme and associated events for the current context.
        /// </summary>
        /// <remarks>This method configures the authentication settings for the current context. Changing
        /// these settings may affect how authentication is processed for subsequent requests.</remarks>
        /// <param name="originalOptionsType">The type that defines the original options used to configure the authentication process. Cannot be null.</param>
        /// <param name="authScheme">The authentication scheme to apply. Determines the method of authentication used for subsequent requests.
        /// Cannot be null or empty.</param>
        /// <param name="originalEvents">An object representing the original event handlers or callbacks associated with the authentication process.
        /// May be null if no events are specified.</param>
        void Set(Type originalOptionsType, string authScheme, object originalEvents);

        /// <summary>
        /// Attempts to retrieve original authentication events based on the specified authentication scheme.
        /// </summary>
        /// <remarks>This method is used for getting original events for authentication scheme
        /// without throwing exceptions.</remarks>
        /// <param name="originalOptionsType">The type of the original options that are being used to determine the authentication events.</param>
        /// <param name="authScheme">The authentication scheme to be used for retrieving the result, which specifies the method of
        /// authentication.</param>
        /// <param name="result">When this method returns <see langword="true"/>, this parameter contains EventType or Events object;
        /// otherwise, it is <see langword="null"/>.</param>
        /// <returns>Returns <see langword="true"/> if the authentication events are successfully retrieved; otherwise, <see
        /// langword="false"/>.</returns>
        bool TryGet(Type originalOptionsType, string authScheme, out object result);
    }
}