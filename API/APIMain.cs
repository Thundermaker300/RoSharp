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
        public virtual string BaseUrl { get; } = "";

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

        internal async Task<HttpResponseMessage> GetAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false)
        {
            HttpMessage reqMessage = new(HttpMethod.Get, string.Concat(baseOverride ?? BaseUrl, url));
            return await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException);
        }

        internal async Task<string> GetStringAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false)
        {
            HttpMessage reqMessage = new(HttpMethod.Get, string.Concat(baseOverride ?? BaseUrl, url));
            return await HttpManager.SendStringAsync(session, reqMessage, verifyApiName, true);
        }

        internal async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            HttpMessage reqMessage = new(HttpMethod.Post, string.Concat(baseOverride ?? BaseUrl, url), data);
            HttpResponseMessage message = await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException, retrying);

            return message;
        }
        internal async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            HttpMessage reqMessage = new(HttpMethod.Patch, string.Concat(baseOverride ?? BaseUrl, url), data);
            HttpResponseMessage message = await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException, retrying);

            return message;
        }
        internal async Task<HttpResponseMessage> DeleteAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            HttpMessage reqMessage = new(HttpMethod.Delete, string.Concat(baseOverride ?? BaseUrl, url));
            HttpResponseMessage message = await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException, retrying);

            return message;
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

            if (!session.LoggedIn)
                throw new ArgumentException("The provided session is not logged in!");

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
