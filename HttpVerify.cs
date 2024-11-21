using Newtonsoft.Json.Linq;
using RoSharp.Exceptions;
using System.Net;

namespace RoSharp
{
    /// <summary>
    /// Class used to verify the response from Roblox APIs
    /// </summary>
    public static class HttpVerify
    {
        /// <summary>
        /// Throws a <see cref="RobloxAPIException"/> if the provided <see cref="HttpResponseMessage"/> failed.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="body">The body of the request. Optional but avoids an extra <see langword="await"/> if available already.</param>
        /// <exception cref="RobloxAPIException">Will always throw if <see cref="HttpResponseMessage.IsSuccessStatusCode"/> is <see langword="false"/>.</exception>
        public static void ThrowIfNecessary(HttpResponseMessage message, string? body = null)
        {
            if (message.IsSuccessStatusCode)
                return;

            if (message.StatusCode == HttpStatusCode.TooManyRequests)
                throw new RobloxAPIException($"Too many requests to the Roblox API. Request url: {message.RequestMessage?.RequestUri} Request method: {message.RequestMessage?.Method}", HttpStatusCode.TooManyRequests);

            body ??= message.Content.ReadAsStringAsync().Result;

            string? userMessage = null;
            dynamic data = JObject.Parse(body);
            if (data.errors != null)
                userMessage =  data.errors[0].message ?? data.errors[0].userFacingMessage ?? message.ReasonPhrase;

            if (userMessage != null)
                userMessage = $"(HTTP {message.StatusCode}) Roblox API error: {userMessage}. Please ensure you have proper permissions to perform this action and try again. Request url: {message.RequestMessage?.RequestUri} Request method: {message.RequestMessage?.Method}";

            RobloxAPIException exception = new(userMessage ?? $"(HTTP {message.StatusCode}) No error message provided by Roblox. Please ensure you have proper permissions to perform this action and try again. Request url: {message.RequestMessage?.RequestUri} Request method: {message.RequestMessage?.Method}", message.StatusCode);
            throw exception;
        }
    }
}
