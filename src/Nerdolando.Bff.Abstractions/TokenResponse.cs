namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// Dto representing a token response from an identity provider.
    /// </summary>
    public class TokenResponse
    {
        /// <summary>
        /// Session identifier associated with the tokens.
        /// </summary>
        public Guid SessionId { get; set; }
        /// <summary>
        /// Access token issued by the identity provider.
        /// </summary>
        public string AccessToken { get; set; } = null!;
        /// <summary>
        /// Refresh token issued by the identity provider.
        /// </summary>
        public string RefreshToken { get; set; } = null!;
        /// <summary>
        /// When access token expires.
        /// </summary>
        public int ExpiresInSeconds { get; set; }
    }
}
