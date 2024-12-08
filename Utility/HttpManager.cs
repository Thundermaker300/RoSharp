using RoSharp.Enums;
using RoSharp.Extensions;
using RoSharp.Structures;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    /// <summary>
    /// The static class responsible for storing <see cref="HttpClient"/>s and sending requests to Roblox.
    /// </summary>
    public static class HttpManager
    {
        private static ConcurrentQueue<HttpClient> httpClients = new ConcurrentQueue<HttpClient>();

        internal static HttpClient GetClient()
        {
            if (httpClients.TryDequeue(out HttpClient? cl))
                return cl;

            HttpClientHandler handler = new();
            handler.UseCookies = false;

            return new(handler);
        }

        internal static void Return(HttpClient client) => httpClients.Enqueue(client);

        /// <summary>
        /// Sends an HTTP request and returns the result.
        /// </summary>
        /// <param name="session">The session object, optional.</param>
        /// <param name="message">The message containing the data to send.</param>
        /// <param name="verifyApiName"></param>
        /// <param name="doNotThrowException"></param>
        /// <param name="retrying">If <see langword="false"/>, will attempt to retry the request again if it fails with <see cref="HttpStatusCode.Forbidden"/> and the <paramref name="session"/> is available. This is so that the <c>x-csrf-token</c> can be retrieved. If <see langword="true"/>, it will not attempt a retry. This behavior will be ignored if the request is a GET.</param>
        /// <returns>A task containing a <see cref="HttpResponseMessage"/> upon completion.</returns>
        [Obsolete("Use SendAsync(Session, HttpMessage)")]
        public static async Task<HttpResponseMessage> SendAsync(Session? session, HttpMessage message, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
            => await SendAsync(session, message);
        public static async Task<HttpResponseMessage> SendAsync(Session? session, HttpMessage message)
        {
            session ??= session.Global();

            if (message.AuthType is AuthType.RobloSecurity or AuthType.RobloSecurityAndApiKey && !message.SilenceExceptions)
                SessionVerify.ThrowIfNecessary(session, message.ApiName ?? "UNKNOWN - MESSAGE DEV");

            if (message.AuthType is AuthType.ApiKey or AuthType.RobloSecurityAndApiKey && !message.SilenceExceptions)
                SessionVerify.ThrowAPIKeyIfNecessary(session, message.ApiName ?? "UNKNOWN - MESSAGE DEV", message.ApiKeyPermission ?? "UNKNOWN - MESSAGE DEV");

            HttpClient client = GetClient();
            HttpRequestMessage messageToSend = new HttpRequestMessage(message.Method, message.Url);

            if (message.Content != null)
            {
                JsonContent content = JsonContent.Create(message.Content);
                messageToSend.Content = content;
                messageToSend.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                messageToSend.Content.Headers.ContentLength = (await content.ReadAsStringAsync()).Length;
            }

            if (message.Headers.Count > 0)
            {
                foreach (var header in message.Headers)
                {
                    if (!messageToSend.Headers.Contains(header.Key))
                        messageToSend.Headers.Add(header.Key, header.Value);
                }
            }    

            if (!string.IsNullOrWhiteSpace(session?.RobloSecurity))
                messageToSend.Headers.Add("Cookie", $".ROBLOSECURITY={session.RobloSecurity}");

            if (!string.IsNullOrWhiteSpace(session?.xcsrfToken))
                messageToSend.Headers.Add("x-csrf-token", session.xcsrfToken);

            if (!string.IsNullOrWhiteSpace(session?.APIKey))
                messageToSend.Headers.Add("x-api-key", session.APIKey);

            HttpResponseMessage resp = await client.SendAsync(messageToSend);
            RoUtility.LogHTTP(session, resp, !message.EnableRetrying, usingV2Request: true);
            Return(client);

            if (resp.StatusCode == HttpStatusCode.Forbidden && message.EnableRetrying && session != null && messageToSend.Method != HttpMethod.Get)
            {
                if (resp.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    message.EnableRetrying = false;
                    return await SendAsync(session, message);
                }
            }

            if (!message.SilenceExceptions)
                HttpVerify.ThrowIfNecessary(resp, await resp.Content.ReadAsStringAsync());

            return resp;
        }

        /// <summary>
        /// Sends an HTTP request and returns its body as a string.
        /// </summary>
        /// <param name="session">The session object, optional.</param>
        /// <param name="message">The message containing the data to send.</param>
        /// <param name="verifyApiName"></param>
        /// <param name="doNotThrowException"></param>
        /// <param name="retrying">If <see langword="false"/>, will attempt to retry the request again if it fails with <see cref="HttpStatusCode.Forbidden"/> and the <paramref name="session"/> is available. This is so that the <c>x-csrf-token</c> can be retrieved. If <see langword="true"/>, it will not attempt a retry. This behavior will be ignored if the request is a GET.</param>
        /// <returns>A task containing the body upon completion.</returns>
        [Obsolete("Use SendStringAsync(Session, HttpMessage)")]
        public static async Task<string> SendStringAsync(Session? session, HttpMessage message, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
            => await SendStringAsync(session, message);
        public static async Task<string> SendStringAsync(Session? session, HttpMessage message)
        {
            bool doSilence = message.SilenceExceptions;
            message.SilenceExceptions = true;

            var response = await SendAsync(session, message);
            string body = await response.Content.ReadAsStringAsync();

            if (!doSilence)
                HttpVerify.ThrowIfNecessary(response, body);

            return body;
        }
    }
}
