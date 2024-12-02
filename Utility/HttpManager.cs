﻿using RoSharp.Extensions;
using RoSharp.Structures;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp.Utility
{
    public static class HttpManager
    {
        private static ConcurrentQueue<HttpClient> httpClients = new ConcurrentQueue<HttpClient>();

        public static HttpClient Get()
        {
            if (httpClients.TryDequeue(out HttpClient? cl))
                return cl;

            HttpClientHandler handler = new();
            handler.UseCookies = false;

            return new(handler);
        }

        public static void Return(HttpClient client) => httpClients.Enqueue(client);

        public static async Task<HttpResponseMessage> SendAsync(Session? session, HttpMessage inputMessage, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            session ??= session.Global();

            if (verifyApiName != null)
                SessionVerify.ThrowIfNecessary(session, verifyApiName);

            HttpClient client = Get();
            HttpRequestMessage message = new HttpRequestMessage(inputMessage.Method, inputMessage.Url);

            if (inputMessage.Content != null)
                message.Content = JsonContent.Create(inputMessage.Content);

            if (!string.IsNullOrWhiteSpace(session?.RobloSecurity))
                message.Headers.Add("Cookie", $".ROBLOSECURITY={session.RobloSecurity}");

            if (!string.IsNullOrWhiteSpace(session?.xcsrfToken))
                message.Headers.Add("x-csrf-token", session.xcsrfToken);

            HttpResponseMessage resp = await client.SendAsync(message);
            RoUtility.LogHTTP(session, resp, client, retrying);
            Return(client);

            if (resp.StatusCode == HttpStatusCode.Forbidden && !retrying && session != null && message.Method != HttpMethod.Get)
            {
                if (resp.Headers.TryGetValues("x-csrf-token", out IEnumerable<string>? headers))
                {
                    session.xcsrfToken = headers.First();
                    return await SendAsync(session, inputMessage, verifyApiName, doNotThrowException, retrying: true);
                }
            }

            if (!doNotThrowException)
                HttpVerify.ThrowIfNecessary(resp, await resp.Content.ReadAsStringAsync());

            return resp;
        }

        public static async Task<string> SendStringAsync(Session? session, HttpMessage message, string? verifyApiName = null, bool doNotThrowException = false, bool retrying = false)
        {
            var response = await SendAsync(session, message, verifyApiName, true, retrying);
            string body = await response.Content.ReadAsStringAsync();

            if (!doNotThrowException)
                HttpVerify.ThrowIfNecessary(response, body);

            return body;
        }
    }
}
