using System.Net;
using System.Net.Http.Json;

namespace RoSharp.API
{
    public class CustomRequest
    {
        private Session? session;
        private Uri uri;

        internal CustomRequest(Session? session) { this.session = session; }

        public void SetBaseUri(string baseUrl)
        {
            SetBaseUri(new Uri(baseUrl));
        }

        public void SetBaseUri(Uri baseUri)
        {
            this.uri = baseUri;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await MakeClient().GetAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, object? body)
        {
            JsonContent content = JsonContent.Create(body);
            return await MakeClient().PostAsync(url, content);
        }

        public async Task<HttpResponseMessage> PatchAsync(string url, object? body)
        {
            JsonContent content = JsonContent.Create(body);
            return await MakeClient().PatchAsync(url, content);
        }

        public async Task<HttpResponseMessage> PutAsync(string url, object? body)
        {
            JsonContent content = JsonContent.Create(body);
            return await MakeClient().PutAsync(url, content);
        }

        private HttpClient MakeClient()
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;

            if (session != null)
                cookies.Add(uri, new Cookie(".ROBLOSECURITY", session.RobloSecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = uri;

            return client;
        }
    }
}
