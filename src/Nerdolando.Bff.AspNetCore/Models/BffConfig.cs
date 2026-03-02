namespace Nerdolando.Bff.AspNetCore.Models
{
    /// <summary>
    /// Configuration options for the BFF library.
    /// </summary>
    public class BffConfig
    {
        /// <summary>
        /// Keeps list of Front URLs that are allowed to access the BFF
        /// Key is front alias of your choice, that you will need to pass to BFF during signing in.
        /// </summary>
        public required Dictionary<string, Uri> FrontUrls { get; set; }

        /// <summary>
        /// Bff endpoint configuration.
        /// </summary>
        public required BffEndpointConfig Endpoints { get; set; } = new BffEndpointConfig();
        /// <summary>
        /// Gets or sets a value indicating whether the ID token should be used as an access token for authorization
        /// purposes.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, the ID token is used in place of an access token
        /// to authorize requests. This may be useful in scenarios where the ID token contains all necessary claims for
        /// access control. Ensure that using the ID token in this way aligns with your security requirements and the
        /// expectations of downstream services.</remarks>
        public required bool UseIdTokenAsAccessToken { get; set; } = false;
    }
}
