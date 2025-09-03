namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// User token storage abstraction.
    /// </summary>
    public interface IUserTokenStorage
    {
        /// <summary>
        /// Stores the user token.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task StoreTokenAsync(UserToken token, CancellationToken ct = default);
        /// <summary>
        /// Reads the user token.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<UserToken?> GetTokenAsync(Guid sessionId, CancellationToken ct = default);

    }
}
