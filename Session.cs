using RoSharp.API;
using RoSharp.Structures;
using System.Net;
using System.Net.Http.Json;

namespace RoSharp
{
    /// <summary>
    /// A session is an object that contains a token used for logging into Roblox for use with authentication endpoints.
    /// </summary>
    public sealed class Session
    {
        private bool loggedIn = false;
        private string roblosecurity = "";
        private SessionAPI? sessionAPI;
        internal string username = "";
        internal string displayname = "";
        internal ulong userid = 0;
        internal DateTime loggedAt;

        public bool LoggedIn => loggedIn;
        public string RobloSecurity => roblosecurity;
        public DateTime LoggedInAt => loggedAt;
        public TimeSpan Elapsed => DateTime.Now - loggedAt;
        public SessionAPI? User
        {
            get
            {
                if (sessionAPI is null)
                {
                    throw new InvalidOperationException("No user associated with this session. Did you call LoginAsync?");
                }
                return sessionAPI;
            }
        }

        public async Task LoginAsync(string roblosecurity)
        {
            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            cookies.Add(UriPages.Users, new Cookie(".ROBLOSECURITY", roblosecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = UriPages.Users;

            HttpResponseMessage authResponse = await client.GetAsync("/v1/users/authenticated");
            if (authResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new AccessViolationException("Login failed.");
            }
            else if (authResponse.IsSuccessStatusCode)
            {
                this.roblosecurity = roblosecurity;

                RobloxLogin result = await authResponse.Content.ReadFromJsonAsync<RobloxLogin>();
                if (result != null)
                {
                    username = result.name;
                    userid = result.id;
                    displayname = result.displayName;
                    loggedIn = true;
                    loggedAt = DateTime.Now;

                    sessionAPI = new SessionAPI(this);
                }
            }
        }

        public CustomRequest MakeCustomRequest()
        {
            CustomRequest custom;
            if (LoggedIn)
            {
                custom = new CustomRequest(this);
            }
            else
            {
                custom = new CustomRequest(null);
            }
            return custom;
        }
    }
}
