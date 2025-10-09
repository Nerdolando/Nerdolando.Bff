using Microsoft.Extensions.DependencyInjection;

namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface IBffBuilder
    {

        /// <summary>
        /// Services collection
        /// </summary>
        IServiceCollection Services { get; }
        /// <summary>
        /// Uses custom token storage implementation for storing user tokens.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IBffBuilder UseCustomTokenStorage<T>() where T : class, IUserTokenStorage;
        /// <summary>
        /// Uses custom token refresher implementation for handling token refresh.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IBffBuilder UseCustomTokenRefresher<T>() where T : class, ITokenRefreshHandler;
    }
}
