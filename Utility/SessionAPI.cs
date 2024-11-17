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

        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("users");

        internal User? user;

        /// <summary>
        /// Gets a <see cref="API.User"/> representing the currently authenticated user.
        /// </summary>
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

        /// <summary>
        /// Gets the gender of the authenticated user.
        /// </summary>
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

        /// <summary>
        /// Gets the country code of the authenticated user.
        /// </summary>
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

        /// <summary>
        /// Gets a <see cref="DateTime"/> representing the set birth date of the user.
        /// </summary>
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

        /// <summary>
        /// Gets the amount of Robux the authenticated user has.
        /// </summary>
        [UsesSession]
        public int Robux
        {
            get
            {
                string rawData = GetString("/v1/user/currency", Constants.URL("economy"));
                dynamic data = JObject.Parse(rawData);
                return data.robux;
            }
        }

        /// <summary>
        /// Sends a friend request to the given target.
        /// </summary>
        /// <param name="targetId">The target user Id.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task SendFriendRequestAsync(ulong targetId)
        {
            await PostAsync($"/v1/contacts/{targetId}/request-friendship", new { });
        }

        /// <summary>
        /// Sends a friend request to the given target.
        /// </summary>
        /// <param name="user">The target user.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task SendFriendRequestAsync(User user) => await SendFriendRequestAsync(user.Id);

        /// <summary>
        /// Unfriends the given target.
        /// </summary>
        /// <param name="targetId">The target user Id.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task UnfriendAsync(ulong targetId) => await PostAsync($"/v1/users/{targetId}/unfriend", new { });

        /// <summary>
        /// Unfriends the given target.
        /// </summary>
        /// <param name="user">The target user.</param>
        /// <returns>A task that completes when the operation is finished.</returns>
        public async Task UnfriendAsync(User user) => await UnfriendAsync(user.Id);

        /// <summary>
        /// Gets a value indicating whether or not the specified experience is favorited by the authenticated user.
        /// </summary>
        /// <param name="experience">The target experience.</param>
        /// <returns>A task that contains a bool when completed.</returns>
        public async Task<bool> FavoritedExperienceAsync(Experience experience) => experience.favoritedByUser; // TODO: This api does not check for matching session.

        /// <summary>
        /// Gets a value indicating whether or not the specified experience is favorited by the authenticated user.
        /// </summary>
        /// <param name="experienceId">The target experience Id.</param>
        /// <returns>A task that contains a bool when completed.</returns>
        public async Task<bool> FavoritedExperienceAsync(ulong experienceId) => await FavoritedExperienceAsync(await Experience.FromId(experienceId, session));

        private string[] incomeSkipList = new[] { "incomingRobuxTotal", "outgoingRobuxTotal" };
        public async Task<EconomyBreakdown> GetIncomeAsync(AnalyticTimeLength timeLength = AnalyticTimeLength.Day)
        {
            var url = $"/v2/users/{session.userid}/transaction-totals?timeFrame={timeLength}&transactionType=summary";
            string rawData = await GetStringAsync(url, Constants.URL("economy"), verifyApiName: "SessionAPI.GetIncomeAsync");
            dynamic data = JObject.Parse(rawData);

            int amount = 0;
            int pending = 0;
            Dictionary<IncomeType, int> breakdown = new();

            foreach (dynamic cat in data)
            {
                string catName = Convert.ToString(cat.Name);

                if (incomeSkipList.Any(sky => catName.ToLower() == sky.ToLower()))
                    continue;

                IncomeType incomeType = Enum.Parse<IncomeType>(catName.Replace("total", string.Empty, StringComparison.OrdinalIgnoreCase), true);
                int myAmount = Convert.ToInt32(cat.Value);
                if (myAmount != 0)
                {
                    breakdown.Add(incomeType, myAmount);

                    if (cat.Name != "pendingRobuxTotal")
                        amount += myAmount;
                    else
                        pending = myAmount;
                }
            }

            return new EconomyBreakdown(timeLength, amount, breakdown, pending);
        }
    }
}
