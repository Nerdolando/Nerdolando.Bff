namespace Nerdolando.Bff.AspNetCore.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class BffEndpointConfig
    {
        /// <summary>
        /// Gets or sets the target API path used for routing requests.
        /// Each request starting with this path will be routed to the target API.
        /// For example: if the target API is https://your-bff.com/api/weather-data and the TargetApiPath is /api, 
        /// this request will be routed to https://your-api.com/api/weather-data
        /// </summary>
        public string TargetApiPath { get; set; } = "/api";
        /// <summary>
        /// This is the base URL of the target API where requests will be forwarded.
        /// </summary>
        public Uri TargetApiBaseUrl { get; set; } = null!;
        /// <summary>
        /// The path that the BFF will use for login requests. This endpoint will initiate the authentication process.
        /// It happens on BFF side, so no routing to the API is done.
        /// </summary>
        public string BffLoginPath { get; set; } = "/login";
        /// <summary>
        /// The path that the BFF will use for logout requests. This endpoint will sign the user out.
        /// It happens on BFF side, so no routing to the API is done.
        /// </summary>
        public string BffLogoutPath { get; set; } = "/logout";
        /// <summary>
        /// The path that the BFF will use to return logged user information based on the current authentication cookie.
        /// It happens on BFF side, so no routing to the API is done.
        /// </summary>
        public string BffUserInfoPath { get; set; } = "/me";
        /// <summary>
        /// Defines the authentication scheme that will be used to challenge unauthenticated users.
        /// </summary>
        public string ChallengeAuthenticationScheme { get; set; } = "oidc";
        /// <summary>
        /// Endpoint used to refresh the access token using the refresh token.
        /// If not set, the refresh token endpoint will be read from OpenID Connect metadata (well known configuration).
        /// It's used by default service implemeting ITokenRefresherHandler.
        /// </summary>
        public Uri? RefreshTokenEndpoint { get; set; }
    }
}
