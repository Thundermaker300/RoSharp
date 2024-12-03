namespace RoSharp.Utility
{
    /// <summary>
    /// Utility functions for RoSharp.
    /// </summary>
    public static class RoUtility
    {
        internal static void LogHTTP(Session? session, HttpResponseMessage message, bool retrying = false)
        {
#if DEBUG
            string body = message.Content.ReadAsStringAsync().Result;

            ConsoleColor color = (message.IsSuccessStatusCode ? ConsoleColor.Cyan : ConsoleColor.Red);
            RoLogger.Debug($"----- BEGIN REQUEST -----", color);
            if (retrying)
                RoLogger.Debug($"<<RETRY REQUEST>>", color);
            RoLogger.Debug($"{message.RequestMessage?.Method} {(message.RequestMessage?.RequestUri?.ToString() ?? "UNKNOWN")}", color);
            RoLogger.Debug($"CODE: HTTP {message.StatusCode} ({(int)message.StatusCode})", color);
            RoLogger.Debug($"AUTH: {!string.IsNullOrWhiteSpace(session?.RobloSecurity)}", color);
            RoLogger.Debug($"XCSRF: {!string.IsNullOrWhiteSpace(session?.xcsrfToken)}", color);
            RoLogger.Debug($"XAPIKEY: {!string.IsNullOrWhiteSpace(session?.APIKey)}", color);
            RoLogger.Debug($"REQUEST BODY: {message.RequestMessage?.Content?.ReadAsStringAsync().Result ?? "NONE"}", color);
            RoLogger.Debug($"REASON PHRASE: HTTP {message.ReasonPhrase}", color);
            RoLogger.Debug($"RESPONSE BODY: {body.Substring(0, Math.Min(body.Length, 200))}", color);
            RoLogger.Debug($"----- END REQUEST -----", color);
#endif
        }
    }
}
