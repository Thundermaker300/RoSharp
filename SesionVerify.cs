namespace RoSharp
{
    /// <summary>
    /// Class used to verify <see cref="Session"/> instances.
    /// </summary>
    public static class SessionVerify
    {
        /// <summary>
        /// Generic message for when an API needs a valid session.
        /// </summary>
        public const string GENERIC_MESSAGE =
            "This API member ({0}) requires a logged-in session to be attached. Did you call AttachSession or a constructor with a session that has been logged in via session.LoginAsync(), or assign a global session?";

        /// <summary>
        /// Generic message for when an API needs a valid session. Includes a "use RefreshAsync()" portion.
        /// </summary>
        public const string GENERIC_REFRESH_MESSAGE =
            "This API member ({0}) requires a logged-in session to be attached. Please attach a logged-in session via AttachSession() and run RefreshAsync() to update the API cache, or add the session when first accessing this class. Alternatively, assign a global session.";
        
        /// <summary>
        /// Generic message for when an API needs a session with an API key.
        /// </summary>
        public const string GENERIC_APIKEY_MESSAGE =
            "This API member ({0}) requires a session containing an API key with the '{1}' permission. Please attach a session via AttachSession() (or add the session when first accessing the class) and call Session.SetAPIKey() before executing this method.";

        /// <summary>
        /// Verifies the provided session and returns if it is authenticated.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>Whether or not it passed verification.</returns>
        public static bool Verify(Session? session)
        {
            if (session is null)
                return false;
            if (!session.LoggedIn)
                return false;

            return true;
        }

        /// <summary>
        /// Verifies the provided session and returns if it has an API key.
        /// </summary>
        /// <param name="session">The session.</param>
        /// <returns>Whether or not it passed verification.</returns>
        public static bool VerifyApiKey(Session? session)
        {
            if (session is null)
                return false;
            if (string.IsNullOrWhiteSpace(session.APIKey))
                return false;

            return true;
        }

        /// <summary>
        /// Calls <see cref="Throw(string)"/> with the given API member name if the session is not verified.
        /// </summary>
        /// <param name="session">Session to verify.</param>
        /// <param name="apiMemberName">API name.</param>
        public static void ThrowIfNecessary(Session? session, string apiMemberName)
        {
            if (!Verify(session))
                Throw(apiMemberName);
        }

        /// <summary>
        /// Throws an exception with the given API member name and API key perm if the session does not have an API key.
        /// </summary>
        /// <param name="session">Session to verify.</param>
        /// <param name="apiMemberName">API name.</param>
        /// <param name="perm">The permission.</param>
        public static void ThrowAPIKeyIfNecessary(Session? session, string apiMemberName, string perm)
        {
            if (!VerifyApiKey(session))
                throw new ArgumentException(string.Format(GENERIC_APIKEY_MESSAGE, apiMemberName, perm), "session");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> with the <see cref="GENERIC_MESSAGE"/> message and the given API name.
        /// </summary>
        /// <param name="apiMemberName">The API name.</param>
        /// <exception cref="ArgumentNullException">Always thrown.</exception>
        public static void Throw(string apiMemberName)
            => throw new ArgumentNullException("session", string.Format(GENERIC_MESSAGE, apiMemberName));

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> with the <see cref="GENERIC_REFRESH_MESSAGE"/> message and the given API name.
        /// </summary>
        /// <param name="apiMemberName">The API name.</param>
        /// <exception cref="ArgumentNullException">Always thrown.</exception>
        public static void ThrowRefresh(string apiMemberName)
            => throw new ArgumentNullException("session", string.Format(GENERIC_REFRESH_MESSAGE, apiMemberName));
    }
}
