using RoSharp.Extensions;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    /// <summary>
    /// The CustomRequest class can be used to make custom requests to the Roblox API using RoSharp's authentication API.
    /// </summary>
    public sealed class CustomRequest
    {
        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/> used in this custom request.
        /// </summary>
        public HttpClient HttpClient { get; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClientHandler"/> used in this custom request.
        /// </summary>
        public HttpClientHandler Handler { get; }
        private Session? session;

        internal CustomRequest(Session? session, string? url)
        {
            this.session = session.Global();

            CookieContainer cookies = new();
            Handler = new HttpClientHandler()
            {
                CookieContainer = cookies,
            };

            HttpClient = new HttpClient(Handler);

            if (url != null)
                SetUrl(url);
        }

        /// <summary>
        /// Sets the new URL to use in this custom request.
        /// </summary>
        /// <param name="url">The new URL to use.</param>
        public void SetUrl(string url)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(url, nameof(url));

            Uri uri = new(url);

            HttpClient.BaseAddress = uri;

            if (session != null)
                Handler.CookieContainer.Add(uri, new Cookie(".ROBLOSECURITY", session.RobloSecurity));
        }

        /// <summary>
        /// Performs a GET request to the set URL.
        /// </summary>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> GetAsync()
        {
            string? filler = null;
            return await HttpClient.GetAsync(filler);
        }

        /// <summary>
        /// Performs a POST request to the set URL.
        /// </summary>
        /// <param name="body">The body to send. RoSharp will convert it to JSON automatically.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> PostAsync(object? body)
        {
            string? filler = null;
            JsonContent content = JsonContent.Create(body);
            return await HttpClient.PostAsync(filler, content);
        }

        /// <summary>
        /// Performs a PATCH request to the set URL.
        /// </summary>
        /// <param name="body">The body to send. RoSharp will convert it to JSON automatically.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> PatchAsync(object? body)
        {
            string? filler = null;
            JsonContent content = JsonContent.Create(body);
            return await HttpClient.PatchAsync(filler, content);
        }

        /// <summary>
        /// Performs a PUT request to the set URL.
        /// </summary>
        /// <param name="body">The body to send. RoSharp will convert it to JSON automatically.</param>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> PutAsync(object? body)
        {
            string? filler = null;
            JsonContent content = JsonContent.Create(body);
            return await HttpClient.PutAsync(filler, content);
        }

        /// <summary>
        /// Performs a DELETE request to the set URL.
        /// </summary>
        /// <returns>A task containing the <see cref="HttpResponseMessage"/> upon completion.</returns>
        public async Task<HttpResponseMessage> DeleteAsync()
        {
            string? filler = null;
            return await HttpClient.DeleteAsync(filler);
        }
    }
}
