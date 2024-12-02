using RoSharp.Interfaces;
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

        internal HttpRequestMessage SetupMessage(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            Uri uri = new(string.Concat(baseOverride ?? BaseUrl, url));

            HttpRequestMessage request = new();
            request.RequestUri = uri;

            return request;
        }

        internal async Task<HttpResponseMessage> GetAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            reqMessage.Method = HttpMethod.Get;

            return await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException);
        }

        internal async Task<string> GetStringAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            reqMessage.Method = HttpMethod.Get;

            return await HttpManager.SendStringAsync(session, reqMessage, verifyApiName, true);
        }

        internal async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);
            reqMessage.Method = HttpMethod.Post;
            reqMessage.Content = content;

            HttpResponseMessage message = await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException, retrying);

            if (message.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null)
            {
                if (message.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await PostAsync(url, data, baseOverride, verifyApiName, doNotThrowException, retrying: true);
                }
            }

            return message;
        }
        internal async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);
            reqMessage.Method = HttpMethod.Patch;
            reqMessage.Content = content;

            HttpResponseMessage message = await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException, retrying);

            if (message.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null)
            {
                if (message.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await PatchAsync(url, data, baseOverride, verifyApiName, doNotThrowException, retrying: true);
                }
            }

            return message;
        }
        internal async Task<HttpResponseMessage> DeleteAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            reqMessage.Method = HttpMethod.Delete;

            HttpResponseMessage message = await HttpManager.SendAsync(session, reqMessage, verifyApiName, doNotThrowException, retrying);

            if (message.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null)
            {
                if (message.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await DeleteAsync(url, baseOverride, verifyApiName, doNotThrowException, retrying: true);
                }
            }

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
