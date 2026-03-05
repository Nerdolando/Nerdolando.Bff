namespace Nerdolando.Bff.Common
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITokenReceivedService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokenResponse"></param>
        /// <returns></returns>
        Task HandleAsync(TokenResponse tokenResponse);
    }
}
