namespace Nerdolando.Bff.Common
{
    /// <summary>
    /// Token refresh handler interface
    /// </summary>
    public interface ITokenRefreshHandler
    {
        /// <summary>
        /// Handles the token refresh process
        /// </summary>
        /// <param name="userToken"></param>
        /// <returns></returns>
        Task<UserToken?> HandleAsync(UserToken userToken);
    }
}
