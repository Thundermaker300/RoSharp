using RoSharp.API;
using RoSharp.Exceptions;
using RoSharp.Structures;
using RoSharp.Utility;
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
        internal string xcsrfToken = "";

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
        /// <exception cref="InvalidOperationException">Session is not authenticated. Did you call LoginAsync?</exception>
        public TimeSpan Elapsed
        {
            get
            {
                if (!LoggedIn)
                    throw new InvalidOperationException("Session is not authenticated. Did you call LoginAsync?");
                return DateTime.Now - LoggedInAt;
            }
        }

        /// <summary>
        /// Gets a <see cref="SessionAPI"/> which contains some API about the current authenticated user.
        /// </summary>
        /// <exception cref="InvalidOperationException">No API associated with this session. Did you call LoginAsync?</exception>
        public SessionAPI? API
        {
            get
            {
                if (sessionAPI is null)
                {
                    throw new InvalidOperationException("No API associated with this session. Did you call LoginAsync?");
                }
                return sessionAPI;
            }
        }

        private User? authUser;

        /// <summary>
        /// Gets a <see cref="User"/> representing the currently authenticated user.
        /// </summary>
        /// <exception cref="InvalidOperationException">No user associated with this session. Did you call LoginAsync?</exception>
        public User? AuthUser
        {
            get
            {
                if (authUser is null)
                {
                    throw new InvalidOperationException("No user associated with this session. Did you call LoginAsync?");
                }
                return authUser;
            }
        }

        /// <summary>
        /// Authenticates this session using the provided .ROBLOSECURITY token.
        /// </summary>
        /// <param name="roblosecurity">The .ROBLOSECURITY token to use for authentication.</param>
        /// <returns>A Task that completes when the operation is finished.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="roblosecurity"/> is null, empty, or whitespace.</exception>
        /// <exception cref="RobloxAPIException">Thrown if the authentication fails.</exception>
        public async Task LoginAsync(string roblosecurity)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(roblosecurity, nameof(roblosecurity));

            HttpRequestMessage message = new(HttpMethod.Get, $"{Constants.URL("users")}/v1/users/authenticated");
            message.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurity}");
            HttpResponseMessage authResponse = await HttpManager.SendAsync(null, message);

            if (authResponse.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new RobloxAPIException("Login failed.");
            }
            else if (authResponse.IsSuccessStatusCode)
            {
                this.roblosecurity = roblosecurity;

                RobloxLogin? result = await authResponse.Content.ReadFromJsonAsync<RobloxLogin>();
                if (result.HasValue)
                {
                    username = result.Value.name;
                    userid = result.Value.id;
                    displayname = result.Value.displayName;
                    loggedIn = true;
                    loggedAt = DateTime.Now;

                    sessionAPI = await SessionAPI.FromSession(this);
                    authUser = await User.FromId(userid);
                }
            }
        }

        /// <summary>
        /// Clears the stored security token and de-authenticates this session.
        /// </summary>
        public void LogoutAsync()
        {
            username = string.Empty;
            userid = 0;
            displayname = string.Empty;
            loggedIn = false;
            loggedAt = null;

            sessionAPI = null;
            authUser = null;

            roblosecurity = string.Empty;
        }
    }
}
