namespace RoSharp.Structures
{
    /// <summary>
    /// Allows configuration of a <see cref="Session"/> and its HTTP calls. All properties can be set and used even if the session <see cref="Session.LoggedIn">is not authenticated</see>.
    /// </summary>
    public class SessionOptions
    {
        /// <summary>
        /// Gets or sets a proxy URL to use that will replace 'roblox.com', or <see langword="null"/> to use no proxy. Defaults to <see langword="null"/>.
        /// </summary>
        public string? ProxyUrl { get; set; } = null;
    }
}
