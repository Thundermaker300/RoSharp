using RoSharp.Exceptions;
using RoSharp.Interfaces;
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

        internal HttpClient MakeHttpClient(string? baseOverride = null, string? verifyApiName = null)
        {
            if (verifyApiName != null)
                SessionVerify.ThrowIfNecessary(session, verifyApiName);

            Uri uri = new Uri(baseOverride != null ? baseOverride : BaseUrl);

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            if (session?.RobloSecurity != string.Empty)
                cookies.Add(uri, new Cookie(".ROBLOSECURITY", session?.RobloSecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = uri;

            return client;
        }

        internal async Task<HttpResponseMessage> GetAsync(string url, string? baseOverride = null, string? verifyApiName = null, bool doNotThrowException = false)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            HttpResponseMessage message = await client.GetAsync(url);

            if (!doNotThrowException)
                HttpVerify.ThrowIfNecessary(message, await message.Content.ReadAsStringAsync());

            return message;
        }

        internal async Task<string> GetStringAsync(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            HttpResponseMessage message = await client.GetAsync(url);
            string body = await message.Content.ReadAsStringAsync();
            HttpVerify.ThrowIfNecessary(message, body);
            return body;
        }

        internal async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PostAsync(url, JsonContent.Create(new { }));

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            HttpResponseMessage response = await client.PostAsync(url, content);
            HttpVerify.ThrowIfNecessary(response, await response.Content.ReadAsStringAsync());
            return response;
        }
        internal async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PatchAsync(url, JsonContent.Create(new { }));

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            HttpResponseMessage response = await client.PatchAsync(url, content);
            HttpVerify.ThrowIfNecessary(response, await response.Content.ReadAsStringAsync());
            return response;
        }
        internal async Task<HttpResponseMessage> DeleteAsync(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);

            HttpResponseMessage initialResponse = await client.DeleteAsync(url);
            //HttpVerify.ThrowIfNecessary(initialResponse);

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            HttpResponseMessage response = await client.DeleteAsync(url);
            HttpVerify.ThrowIfNecessary(response, await response.Content.ReadAsStringAsync());
            return response;
        }

        internal Session? session;

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
