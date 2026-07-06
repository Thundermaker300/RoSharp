using RoSharp.API;
using RoSharp.Enums;
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
        private string apiKey = "";
        private SessionAPI? sessionAPI;
        internal string username = "";
        internal string displayname = "";
        internal ulong userid = 0;
        internal DateTime? loggedAt;
        internal string xcsrfToken = "";

        internal string RobloSecurity => roblosecurity;

        internal string APIKey => apiKey;

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
                    throw new InvalidOperationException("Session is not authenticated. Did you call LoginAsync or LoginWithAPIKeyAsync?");
                return DateTime.Now - LoggedInAt;
            }
        }

        private AuthenticationMethod authMethod;

        /// <summary>
        /// Indicates how this session has been authenticated.
        /// </summary>
        public AuthenticationMethod AuthMethod => authMethod;

        /// <summary>
        /// Gets a <see cref="SessionAPI"/> which contains some API about the current authenticated user. This will be <see langword="null"/> if the session is not logged in via <see cref="LoginAsync(string)"/>.
        /// </summary>
        public SessionAPI? API
            => sessionAPI;

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
        /// Gets or sets options for this session.
        /// </summary>
        public SessionOptions? Options { get; set; }

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

            HttpClientHandler handler = new()
            {
                UseCookies = false
            };

            HttpClient client = new(handler);

            HttpRequestMessage message = new(HttpMethod.Get, $"{Constants.URL("users")}/v1/users/authenticated");
            message.Headers.Add("Cookie", $".ROBLOSECURITY={roblosecurity}");

            HttpResponseMessage authResponse = await client.SendAsync(message);

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
                    authMethod = AuthenticationMethod.RobloSecurity;

                    sessionAPI = await SessionAPI.FromSession(this);
                    authUser = await User.FromId(userid, this);
                }
            }
        }

        /// <summary>
        /// Authenticates this session using the provided API key.
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <returns>A Task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Thrown if the authentication fails.</exception>
        public async Task LoginWithAPIKeyAsync(string apiKey)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(apiKey, nameof(apiKey));

            HttpClient client = new();

            HttpRequestMessage message = new(HttpMethod.Post, $"{Constants.URL("apis")}/api-keys/v1/introspect")
            {
                Content = JsonContent.Create(new
                {
                    apiKey = apiKey,
                })
            };

            HttpResponseMessage authResponse = await client.SendAsync(message);

            if (authResponse.StatusCode == HttpStatusCode.BadRequest)
            {
                throw new RobloxAPIException("Invalid API key.");
            }
            else if (authResponse.IsSuccessStatusCode)
            {
                this.apiKey = apiKey;

                loggedIn = true;
                loggedAt = DateTime.Now;
                authMethod = AuthenticationMethod.ApiKey;
            }
        }

        /// <summary>
        /// Adds an API key to this session.
        /// </summary>
        /// <param name="apiKey">The API key to add, or <see langword="null"/> to remove the API key.</param>
        [Obsolete("Use LoginWithAPIKeyAsync")]
        public void SetAPIKey(string apiKey)
            => this.apiKey = apiKey;

        /// <summary>
        /// Clears all stored security tokens and de-authenticates this session.
        /// </summary>
        public void LogoutAsync()
        {
            username = string.Empty;
            userid = 0;
            displayname = string.Empty;
            loggedIn = false;
            loggedAt = null;
            authMethod = AuthenticationMethod.Unauthenticated;

            sessionAPI = null;
            authUser = null;

            roblosecurity = string.Empty;
            apiKey = string.Empty;
        }
    }
}
