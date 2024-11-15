using System.Net;
using System.Net.Http.Json;

namespace RoSharp.API
{
    /// <summary>
    /// The CustomRequest class can be used to make custom requests to the Roblox API using RoSharp's authentication API.
    /// </summary>
    public class CustomRequest
    {
        private Session? session;

        internal CustomRequest(Session? session) { this.session = session; }

        /// <summary>
        /// Performs a GET request to the provided URL.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await MakeClient(url).GetAsync(url);
        }

        /// <summary>
        /// Performs a POST request to the provided URL.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="body">The body to send. RoSharp will convert it to JSON automatically.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> PostAsync(string url, object? body)
        {
            JsonContent content = JsonContent.Create(body);
            return await MakeClient(url).PostAsync(url, content);
        }

        /// <summary>
        /// Performs a PATCH request to the provided URL.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="body">The body to send. RoSharp will convert it to JSON automatically.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> PatchAsync(string url, object? body)
        {
            JsonContent content = JsonContent.Create(body);
            return await MakeClient(url).PatchAsync(url, content);
        }

        /// <summary>
        /// Performs a PUT request to the provided URL.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <param name="body">The body to send. RoSharp will convert it to JSON automatically.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> PutAsync(string url, object? body)
        {
            JsonContent content = JsonContent.Create(body);
            return await MakeClient(url).PutAsync(url, content);
        }

        /// <summary>
        /// Performs a DELETE request to the provided URL.
        /// </summary>
        /// <param name="url">The URL to send the request to.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await MakeClient(url).DeleteAsync(url);
        }

        private HttpClient MakeClient(string url)
        {
            Uri uri = new(url);

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
