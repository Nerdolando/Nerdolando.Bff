namespace Nerdolando.Bff.Components.Abstractions
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILogOutRequest
    {
        /// <summary>
        /// Logs the user out and redirects to the specified return URL.
        /// Return URL must be local URL.
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        Task LogoutAsync(string returnUrl = "/");
    }
}
