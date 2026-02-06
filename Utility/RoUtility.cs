using RoSharp.Structures;

namespace RoSharp.Utility
{
    /// <summary>
    /// Utility functions for RoSharp.
    /// </summary>
    public static class RoUtility
    {
        /// <summary>
        /// If the <paramref name="input"/> is <see langword="null"/>, empty, or whitespace, returns <see langword="null"/>. Otherwise, returns <paramref name="input"/>.
        /// </summary>
        /// <param name="input">The input to test.</param>
        /// <returns>The input or null.</returns>
        public static string? GetInputOrNull(string? input)
            => string.IsNullOrWhiteSpace(input) ? null : input;

        internal static void LogHTTP(Session? session, HttpMessage sendMessage, HttpResponseMessage message, bool retrying = false)
        {
#if DEBUG
            string body = message.Content.ReadAsStringAsync().Result;

            ConsoleColor color = (message.IsSuccessStatusCode ? ConsoleColor.Cyan : ConsoleColor.Red);
            RoLogger.Debug($"----- BEGIN REQUEST -----", color);
            if (retrying)
                RoLogger.Debug($"<<RETRY REQUEST>>", color);
            RoLogger.Debug($">>> REQUEST <<<", color);
            RoLogger.Debug($"{sendMessage.Method} {(sendMessage.Url ?? "UNKNOWN")}", color);
            RoLogger.Debug($"ROBLOSECURITY AUTH: {!string.IsNullOrWhiteSpace(session?.RobloSecurity)}", color);
            RoLogger.Debug($"XCSRF: {!string.IsNullOrWhiteSpace(session?.xcsrfToken)}", color);
            RoLogger.Debug($"XAPIKEY: {!string.IsNullOrWhiteSpace(session?.APIKey)}", color);
            RoLogger.Debug($"REQUEST BODY: {message.RequestMessage?.Content?.ReadAsStringAsync().Result ?? "NONE"}", color); ;
            RoLogger.Debug($"AUTHTYPE REQ: {sendMessage.AuthType}", color);
            RoLogger.Debug($"EXCEPTIONS SILENCED: {sendMessage.SilenceExceptions}", color);
            RoLogger.Debug($">>> RESPONSE <<<", color);
            RoLogger.Debug($"SUCCESS: {message.IsSuccessStatusCode}", color);
            RoLogger.Debug($"CODE: HTTP {message.StatusCode} ({(int)message.StatusCode})", color);
            RoLogger.Debug($"REASON PHRASE: HTTP {message.ReasonPhrase}", color);
            RoLogger.Debug($"RESPONSE BODY: {body.Substring(0, Math.Min(body.Length, 200))}", color);
            RoLogger.Debug($"----- END REQUEST -----", color);
            RoLogger.Debug(string.Empty, color);
#endif
        }
    }
}
