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
    }
}
