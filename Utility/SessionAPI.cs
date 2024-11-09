using RoSharp.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Pooling;

namespace RoSharp.API
{
    public class SessionAPI : APIMain
    {
        public SessionAPI(Session session) : base(session) { }

        public override string BaseUrl => "https://users.roblox.com";

        private User? userInfo;

        [UsesSession]
        public User UserInfo
        {
            get
            {
                if (userInfo is null)
                {
                    SessionErrors.Verify(session);
                    userInfo = RoPool<User>.Get(session.userid, session);
                }
                return userInfo;
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

        public async Task UnfriendAsync(ulong targetId)
        {
            await PostAsync($"/v1/users/{targetId}/unfriend", new { });
        }

        public async Task UnfriendAsync(User user) => await UnfriendAsync(user.Id);

        public bool FavoritedExperience(Experience experience) => experience.favoritedByUser;
        public bool FavoritedExperience(ulong experienceId) => new Experience(experienceId, session).favoritedByUser;
    }
}
