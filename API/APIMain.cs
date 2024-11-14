using RoSharp.Interfaces;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp.API
{
    public class APIMain
    {
        public virtual string BaseUrl { get; } = "";

        public APIMain(Session session)
        {
            try
            {
                AttachSession(session);
            }
            catch { }
        }

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

        internal async Task<HttpResponseMessage> GetAsync(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            HttpResponseMessage message = await client.GetAsync(url);
            if (message.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }
            return message;
        }


        [Obsolete("Use async version where possible")]
        internal HttpResponseMessage Get(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            HttpResponseMessage message = client.GetAsync(url).Result;
            if (message.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }
            return message;
        }

        [Obsolete("Use async version where possible")]
        internal string GetString(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            HttpResponseMessage message = client.GetAsync(url).Result;
            if (message.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }
            return message.Content.ReadAsStringAsync().Result;
        }

        internal async Task<string> GetStringAsync(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            HttpResponseMessage message = await client.GetAsync(url);
            if (message.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }
            return await message.Content.ReadAsStringAsync();
        }

        internal async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PostAsync(url, null);

            if (initialResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            return await client.PostAsync(url, content);
        }
        internal async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PatchAsync(url, null);

            if (initialResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            return await client.PatchAsync(url, content);
        }
        internal async Task<HttpResponseMessage> DeleteAsync(string url, string? baseOverride = null, string? verifyApiName = null)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifyApiName);

            HttpResponseMessage initialResponse = await client.DeleteAsync(url);

            if (initialResponse.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new HttpRequestException("Too many requests.");
            }

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            return await client.DeleteAsync(url);
        }

        internal Session? session;
        public virtual void AttachSession(Session session, bool refreshIfPossible = false)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (!session.LoggedIn)
                throw new InvalidOperationException("The provided session is not logged in!");

            this.session = session;

            if (refreshIfPossible && this is IRefreshable refreshable)
            {
                refreshable.RefreshAsync().Wait();
            }
        }

        public virtual void DetachSession()
        {
            session = null;
        }
    }
}
