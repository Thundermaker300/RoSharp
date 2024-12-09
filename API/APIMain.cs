using RoSharp.Interfaces;
using RoSharp.Structures;
using RoSharp.Utility;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp.API
{
    /// <summary>
    /// The base class for all API members that use <see cref="Session"/> instances for authentication.
    /// </summary>
    public class APIMain
    {
        /// <summary>
        /// Gets the base URL for API requests for this instance type.
        /// </summary>
        public virtual string BaseUrl { get; } = string.Empty;

        /// <summary>
        /// Instantiates a new <see cref="APIMain"/> with the given session.
        /// </summary>
        /// <param name="session">The session to instantiate with.</param>
        public APIMain(Session session)
        {
            try
            {
                AttachSession(session);
            }
            catch { }
        }

        /// <summary>
        /// Instantiates a new <see cref="APIMain"/> without a session.
        /// </summary>
        public APIMain() { }

        internal Session? session;

        internal async Task<HttpResponseMessage> SendAsync(HttpMessage message, string? baseOverride = null)
        {
            message.Url = string.Concat(baseOverride ?? BaseUrl, message.Url);
            return await HttpManager.SendAsync(session, message);
        }

        internal async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string? baseOverride = null, object? body = null)
        {
            var message = new HttpMessage(method, url, body);
            return await SendAsync(message, baseOverride);
        }

        internal async Task<string> SendStringAsync(HttpMessage message, string? baseOverride = null)
        {
            message.Url = string.Concat(baseOverride ?? BaseUrl, message.Url);
            return await HttpManager.SendStringAsync(session, message);
        }

        internal async Task<string> SendStringAsync(HttpMethod method, string url, string? baseOverride = null, object? body = null)
        {
            var message = new HttpMessage(method, url, body);
            return await SendStringAsync(message, baseOverride);
        }

        /// <summary>
        /// Attaches a session object to this API member, allowing authentication-required endpoints to be used.
        /// </summary>
        /// <param name="session">The session to attach.</param>
        /// <param name="refreshIfPossible">If true, will yield the current thread to run <see cref="IRefreshable.RefreshAsync"/>, if this instance is refreshable.</param>
        /// <exception cref="ArgumentException">The provided session is not logged in!</exception>
        public virtual void AttachSession(Session session, bool refreshIfPossible = false)
        {
            ArgumentNullException.ThrowIfNull(session);

            //if (!session.LoggedIn)
            //    throw new ArgumentException("The provided session is not logged in!");

            this.session = session;

            if (refreshIfPossible && this is IRefreshable refreshable)
            {
                refreshable.RefreshAsync().Wait();
            }
        }

        /// <summary>
        /// Removes the attached session object, de-authenticating this instance from the Roblox API.
        /// </summary>
        public virtual void DetachSession()
        {
            session = null;
        }
    }
}
