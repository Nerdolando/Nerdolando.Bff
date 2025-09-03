namespace Nerdolando.Bff.Components.Abstractions
{
    /// <summary>
    /// Interface that gives you the ability to refresh the authentication state.
    /// </summary>
    public interface IAuthenticationStateRefresher
    {
        /// <summary>
        /// Refreshes the authentication state. Should be called when you want to update the authentication state.
        /// </summary>
        /// <returns></returns>
        Task RefreshAsync();
    }
}
