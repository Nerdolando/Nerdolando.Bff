namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// Dto representing a user token. This model is stored in the token storage.
    /// </summary>
    public class UserToken
    {
        /// <summary>
        /// Session identifier associated with the user token.
        /// </summary>
        public Guid SessionId { get; set; }
        /// <summary>
        /// Access token.
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;
        /// <summary>
        /// Refesh token.
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;
        /// <summary>
        /// When the access token expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }
    }
}
