using System.ComponentModel.DataAnnotations;

namespace Nerdolando.Bff.Components.Models
{
    /// <summary>
    /// Client options for BFF integration in Blazor applications.
    /// </summary>
    public class ClientBffOptions
    {
        /// <summary>
        /// Alias for front application. It should match one of the keys in FrontUrls defined in BffConfig on the server side.
        /// BFF will only respond to URL defined by this alias.
        /// </summary>
        [Required]
        public string FrontAlias { get; set; } = "front";
        /// <summary>
        /// Base address of the BFF server.
        /// </summary>
        [Required]
        public Uri BffBaseAddress { get; set; } = null!;
        /// <summary>
        /// Login path on the BFF server. Default is /login.
        /// </summary>
        [Required]
        public string BffLoginPath { get; set; } = "/login";

        /// <summary>
        /// Logout path on the BFF server. Default is /logout.
        /// </summary>
        [Required]
        public string BffLogoutPath { get; set; } = "/logout";

        /// <summary>
        /// Path for retrieving user information from the BFF server. Default is /me.
        /// </summary>
        [Required]
        public string BffUserInfoPath { get; set; } = "/me";

        /// <summary>
        /// Gets or sets a value indicating whether the user should be logged out using a backchannel request to the
        /// authentication server.
        /// </summary>
        /// <remarks>When set to <see langword="true"/>, the logout process uses a backchannel
        /// communication with the authentication server, which may provide a more seamless user experience. Ensure that
        /// the application and authentication server are configured to support backchannel logout before enabling this
        /// option.
        /// 
        /// Normally logout should be executed by frontchannel, that is simple redirect from browser. However, in some cases, 
        /// such as when the authentication server does not support frontchannel logout, 
        /// backchannel logout can be used. With backchannel logout, the client application sends a request to Bff using AJAX request. 
        /// </remarks>
        public bool LogoutWithBackchannel { get; set; } = false;
    }
}
