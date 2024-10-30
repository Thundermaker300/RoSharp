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

        public HttpResponseMessage Get(string url, string? baseOverride = null, bool verifySession = true)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifySession);
            return client.GetAsync(url).Result;
        }

        public string GetString(string url, string? baseOverride = null, bool verifySession = true)
        {
            HttpClient client = MakeHttpClient(baseOverride, verifySession);
            HttpResponseMessage response = client.GetAsync(url).Result;
            return response.Content.ReadAsStringAsync().Result;
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object data, string? baseOverride = null)
        {
            HttpClient client = MakeHttpClient(baseOverride);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PostAsync(url, null);

            if (initialResponse.Headers.GetValues("x-csrf-token").Any())
                client.DefaultRequestHeaders.Add("x-csrf-token", initialResponse.Headers.GetValues("x-csrf-token").First());

            return await client.PostAsync(url, content);
        }
        public async Task<HttpResponseMessage> PatchAsync(string url, object data, string? baseOverride = null)
        {
            HttpClient client = MakeHttpClient(baseOverride);
            JsonContent content = JsonContent.Create(data);

            HttpResponseMessage initialResponse = await client.PatchAsync(url, null);

            if (initialResponse.Headers.GetValues("x-csrf-token").Any())
                client.DefaultRequestHeaders.Add("x-csrf-token", initialResponse.Headers.GetValues("x-csrf-token").First());
            return await client.PatchAsync(url, content);
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
