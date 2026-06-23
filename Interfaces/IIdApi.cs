using RoSharp.Exceptions;

namespace RoSharp.Interfaces
{
    /// <summary>
    /// Interface for classes that have an Id parameter.
    /// </summary>
    /// <typeparam name="T">The type of the class.</typeparam>
    public interface IIdApi<T>
    {
        /// <summary>
        /// Gets the ID of the instance.
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// Gets the Web URL that points to the asset.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> from the provided Id. The returned class represents a <typeparamref name="T"/> from the Roblox API.
        /// </summary>
        /// <param name="id">The unique Id of the <typeparamref name="T"/>.</param>
        /// <param name="session">Session. Optional.</param>
        /// <param name="refresh">If true, the refresh method will be called automatically, which retrieves information from Roblox and validates that the asset exists. If false, this step is skipped, speeding up load times, however most properties will be empty or default values. This value should remain true unless you plan to call refresh at a later date or know what you're doing. Note: Classes that are not refreshed will not be added to the pooling system, so they will not be cached.</param>
        /// <returns>A task containing the <typeparamref name="T"/>.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public abstract static Task<T> FromId(ulong id, Session? session, bool refresh);

        /// <summary>
        /// Attaches a session to this instance and returns it.
        /// </summary>
        /// <param name="session">The session to attach.</param>
        /// <returns>The same instance.</returns>
        public T AttachSessionAndReturn(Session? session);
    }
}
