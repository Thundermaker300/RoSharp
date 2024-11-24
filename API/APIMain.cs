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

        private HttpClient httpClient;
        private HttpClientHandler httpHandler;

        internal HttpRequestMessage SetupMessage(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            if (verifyApiName != null)
                SessionVerify.ThrowIfNecessary(session, verifyApiName);

            Uri uri = new(string.Concat(baseOverride ?? BaseUrl, url));

            if (httpClient == null)
            {
                httpHandler = new HttpClientHandler();
                httpHandler.UseCookies = false;

                httpClient = new HttpClient(httpHandler);
            }

            HttpRequestMessage request = new();
            request.RequestUri = uri;

            if (session?.RobloSecurity != string.Empty)
                request.Headers.Add("Cookie", $".ROBLOSECURITY={session?.RobloSecurity}");

            if (!string.IsNullOrWhiteSpace(session?.xcsrfToken))
                request.Headers.Add("x-csrf-token", session?.xcsrfToken);

            return request;
        }

        internal async Task<HttpResponseMessage> GetAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            reqMessage.Method = HttpMethod.Get;

            HttpResponseMessage message = await httpClient.SendAsync(reqMessage);
            RoUtility.LogHTTP(session, message, httpClient);

            if (!doNotThrowException)
                HttpVerify.ThrowIfNecessary(message, await message.Content.ReadAsStringAsync());

            return message;
        }

        internal async Task<string> GetStringAsync(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            reqMessage.Method = HttpMethod.Get;

            HttpResponseMessage message = await httpClient.SendAsync(reqMessage);

            RoUtility.LogHTTP(session, message, httpClient);

            string body = await message.Content.ReadAsStringAsync();
            HttpVerify.ThrowIfNecessary(message, body);
            return body;
        }

        internal async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null, bool retrying = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);
            reqMessage.Method = HttpMethod.Post;
            reqMessage.Content = content;

            HttpResponseMessage message = await httpClient.SendAsync(reqMessage);

            RoUtility.LogHTTP(session, message, httpClient, retrying);

            if (message.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null)
            {
                if (message.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await PostAsync(url, data, baseOverride, verifyApiName, true);
                }
            }

            HttpVerify.ThrowIfNecessary(message, await message.Content.ReadAsStringAsync());
            return message;
        }
        internal async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null, bool retrying = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);
            reqMessage.Method = HttpMethod.Patch;
            reqMessage.Content = content;

            HttpResponseMessage message = await httpClient.SendAsync(reqMessage);

            RoUtility.LogHTTP(session, message, httpClient, retrying);

            if (message.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null)
            {
                if (message.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await PatchAsync(url, data, baseOverride, verifyApiName, true);
                }
            }

            HttpVerify.ThrowIfNecessary(message, await message.Content.ReadAsStringAsync());
            return message;
        }
        internal async Task<HttpResponseMessage> DeleteAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool retrying = false)
        {
            HttpRequestMessage reqMessage = SetupMessage(url, baseOverride, verifyApiName);
            reqMessage.Method = HttpMethod.Delete;

            HttpResponseMessage message = await httpClient.SendAsync(reqMessage);
            RoUtility.LogHTTP(session, message, httpClient, retrying);

            if (message.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null)
            {
                if (message.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await DeleteAsync(url, baseOverride, verifyApiName, true);
                }
            }

            HttpVerify.ThrowIfNecessary(message, await message.Content.ReadAsStringAsync());
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
