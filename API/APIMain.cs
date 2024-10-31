using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

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

        public HttpClient MakeHttpClient(string? baseOverride = null, bool verifySession = true)
        {
            bool isSessionGood = false;
            if (verifySession)
                isSessionGood = SessionErrors.Verify(session);

            Uri uri = new Uri(baseOverride != null ? baseOverride : BaseUrl);

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            if (isSessionGood)
                cookies.Add(uri, new Cookie(".ROBLOSECURITY", session.RobloSecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = uri;

            return client;
        }

        internal HttpResponseMessage Get(string url, string? baseOverride = null, bool verifySession = true)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifySession);
            return client.GetAsync(url).Result;
        }

        internal string GetString(string url, string? baseOverride = null, bool verifySession = true)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifySession);
            HttpResponseMessage response = client.GetAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        internal async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null)
        {
            HttpClient client = MakeHttpClient(baseOverride);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PostAsync(url, null);

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            return await client.PostAsync(url, content);
        }
        internal async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null)
        {
            HttpClient client = MakeHttpClient(baseOverride);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PatchAsync(url, null);

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            return await client.PatchAsync(url, content);
        }
        internal async Task<HttpResponseMessage> DeleteAsync(string url, string? baseOverride = null)
        {
            HttpClient client = MakeHttpClient(baseOverride);

            HttpResponseMessage initialResponse = await client.DeleteAsync(url);

            if (initialResponse.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                client.DefaultRequestHeaders.Add("x-csrf-token", headers.First());

            return await client.DeleteAsync(url);
        }

        protected Session? session;
        public virtual void AttachSession(Session session)
        {
            ArgumentNullException.ThrowIfNull(session);

            if (!session.LoggedIn)
                throw new InvalidOperationException("The provided session is not logged in!");

            this.session = session;
        }

        public virtual void DetachSession()
        {
            session = null;
        }
    }
}
