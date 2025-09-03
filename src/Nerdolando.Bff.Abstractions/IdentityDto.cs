namespace Nerdolando.Bff.Abstractions
{
    /// <summary>
    /// Identity claim DTO
    /// </summary>
    public class IdentityClaim
    {
        /// <summary>
        /// Claim type
        /// </summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Claim value
        /// </summary>
        public string Value { get; set; } = string.Empty;
    }

    /// <summary>
    /// Identity Dto to return user information
    /// </summary>
    public class IdentityDto
    {
        /// <summary>
        /// Authentication type
        /// </summary>
        public string AuthenticationType { get; set; } = string.Empty;
        /// <summary>
        /// User claims
        /// </summary>
        public IEnumerable<IdentityClaim> Claims { get; set; } = Array.Empty<IdentityClaim>();
    }
}
