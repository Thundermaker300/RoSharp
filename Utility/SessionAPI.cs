using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.Enums;

namespace RoSharp.API
{
    // TODO: The API in this class needs converted to use the new system (SessionVerify.ThrowIfNecessary)
    // The API should also all be obtained in a ctor or refresh method instead of each access.
    public class SessionAPI : APIMain
    {
        public SessionAPI(Session session) : base(session) { }

        public override string BaseUrl => "https://users.roblox.com";

        private User? user;

        [UsesSession]
        public User User
        {
            get
            {
                if (user is null)
                {
                    SessionVerify.ThrowIfNecessary(session, "SessionAPI.User");
                    user = User.FromId(session.userid, session).Result;
                }
                return user;
            }
        }

        [UsesSession]
        public Gender Gender
        {
            get
            {
                string rawData = GetString("/v1/gender");
                dynamic data = JObject.Parse(rawData);
                return ((Gender[])Enum.GetValues(typeof(Enums.Gender)))[data.gender];
            }
        }

        [UsesSession]
        public string CountryCode
        {
            get
            {
                string rawData = GetString("/v1/users/authenticated/country-code");
                dynamic data = JObject.Parse(rawData);
                return data.countryCode;
            }
        }

        [UsesSession]
        public DateTime BirthDate
        {
            get
            {
                string rawData = GetString("/v1/birthdate");
                dynamic data = JObject.Parse(rawData);
                return new DateTime((int)data.birthYear, (int)data.birthMonth, (int)data.birthDay);
            }
        }

        [UsesSession]
        public int Robux
        {
            get
            {
                string rawData = GetString("/v1/user/currency", "https://economy.roblox.com");
                dynamic data = JObject.Parse(rawData);
                return data.robux;
            }
        }

        public async Task SendFriendRequestAsync(ulong targetId)
        {
            await PostAsync($"/v1/contacts/{targetId}/request-friendship", new { });
        }

        public async Task SendFriendRequestAsync(User user) => await SendFriendRequestAsync(user.Id);

        public async Task UnfriendAsync(ulong targetId) => await PostAsync($"/v1/users/{targetId}/unfriend", new { });

        public async Task UnfriendAsync(User user) => await UnfriendAsync(user.Id);

        public async Task<bool> FavoritedExperienceAsync(Experience experience) => experience.favoritedByUser; // TODO: This api does not check for matching session.
        public async Task<bool> FavoritedExperienceAsync(ulong experienceId) => await FavoritedExperienceAsync(await Experience.FromId(experienceId, session));
    }
}
