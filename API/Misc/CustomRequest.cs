using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RoSharp.API
{
    public class CustomRequest
    {
        private Session? session;
        private Uri uri;

        internal CustomRequest(Session? session) { this.session = session; }

        public void SetUri(string baseUrl)
        {
            SetUri(new Uri(baseUrl));
        }

        public void SetUri(Uri baseUri)
        {
            this.uri = baseUri;
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            return await MakeClient().GetAsync(url);
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
