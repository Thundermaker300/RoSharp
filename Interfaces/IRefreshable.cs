using RoSharp.Exceptions;

namespace RoSharp.Interfaces
{
    /// <summary>
    /// Indicates a type that has cached API that can be forcefully refreshed.
    /// </summary>
    public interface IRefreshable
    {
        /// <summary>
        /// Gets or sets the last time this API was updated.
        /// </summary>
        public DateTime RefreshedAt { get; set; }

        /// <summary>
        /// Forces this member's API that is retrieved from Roblox.
        /// </summary>
        /// <returns>A task that completes when API has been fully updated.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure.</exception>
        public Task RefreshAsync();
    }
}
