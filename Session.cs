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
        internal DateTime? loggedAt;

        internal string RobloSecurity => roblosecurity;

        /// <summary>
        /// Indicates if this session has been logged in.
        /// </summary>
        public bool LoggedIn => loggedIn;

        /// <summary>
        /// If <see cref="LoggedIn"/> is true, this contains a <see cref="DateTime"/> representing when the Session was authenticated.
        /// </summary>
        public DateTime LoggedInAt => loggedAt.GetValueOrDefault();

        /// <summary>
        /// If <see cref="LoggedIn"/> is true, this contains a <see cref="TimeSpan"/> representing the length of time this session has been authenticated for.
        /// </summary>
        public TimeSpan Elapsed => DateTime.Now - LoggedInAt;

        /// <summary>
        /// Gets a <see cref="SessionAPI"/> which contains some API about the current authenticated user.
        /// </summary>
        public SessionAPI? API
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

        /// <summary>
        /// Authenticates this session using the provided .ROBLOSECURITY token.
        /// </summary>
        /// <param name="roblosecurity">The .ROBLOSECURITY token to use for authentication.</param>
        /// <returns>A Task that completes when the operation is finished.</returns>
        /// <exception cref="AccessViolationException">Thrown if the authentication fails.</exception>
        public async Task LoginAsync(string roblosecurity)
        {
            Uri uri = new(Constants.URL("users"));

            CookieContainer cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler();
            handler.CookieContainer = cookies;
            cookies.Add(uri, new Cookie(".ROBLOSECURITY", roblosecurity));

            HttpClient client = new HttpClient(handler);
            client.BaseAddress = uri;

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
                    sessionAPI.user = await User.FromId(userid, this);
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
