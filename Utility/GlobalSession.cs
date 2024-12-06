namespace RoSharp.Utility
{
    /// <summary>
    /// The GlobalSession class allows the program to define one <see cref="Session"/> instance to use for the entire duration of the program, eliminating the need to provide session instances to every API call.
    /// </summary>
    public static class GlobalSession
    {
        private static Session? assigned;

        /// <summary>
        /// Gets the <see cref="Session"/> that is currently assigned as the global session. Can be <see langword="null"/>.
        /// </summary>
        public static Session? Assigned => assigned;

        /// <summary>
        /// Assigns a session to be used as the global session.
        /// </summary>
        /// <param name="session">The session to use.</param>
        /// <exception cref="ArgumentException">Will throw if the session is not authenticated and does not have an API key.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="session"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">Will throw if the provided session is already assigned as the current session.</exception>
        /// <remarks>The provided session cannot be <see langword="null"/> and must be authenticated by <see cref="Session.LoginAsync(string)"/> OR have an attached API key before calling this method.</remarks>
        public static void AssignSession(Session session)
        {
            ArgumentNullException.ThrowIfNull(session, nameof(session));

            if (session.Equals(assigned))
                throw new InvalidOperationException("The provided session is already assigned as the global session!");
            if (!SessionVerify.Verify(session) && session.APIKey == string.Empty)
                throw new ArgumentException("GlobalSession.AssignSession requires a non-null & authenticated session (or a session with an API key).");

            assigned = session;
        }

        /// <summary>
        /// Removes the currently-assigned global session.
        /// </summary>
        public static void Unassign() => assigned = null;
    }
}
