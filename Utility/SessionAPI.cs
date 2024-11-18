using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.Enums;
using RoSharp.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RoSharp.API
{
    // TODO: The API in this class needs converted to use the new system (SessionVerify.ThrowIfNecessary)
    // The API should also all be obtained in a ctor or refresh method instead of each access.
    public class SessionAPI : APIMain, IRefreshable
    {
        public DateTime RefreshedAt { get; set; }

        private SessionAPI(Session session) : base(session) { }

        public static async Task<SessionAPI> FromSession(Session session)
        {
            if (!SessionVerify.Verify(session))
                throw new ArgumentException("Session cannot be null and must be authenticated.", nameof(session));

            SessionAPI api = new(session);
            await api.RefreshAsync();

            return api;
        }

        public async Task RefreshAsync()
        {
            user = await User.FromId(session.userid, session);

            dynamic genderData = JObject.Parse(await GetStringAsync("/v1/gender"));
            gender = (Gender)Convert.ToInt32(genderData.gender);

            dynamic countryCodeData = JObject.Parse(await GetStringAsync("/v1/users/authenticated/country-code"));
            countryCode = countryCodeData.countryCode;

            dynamic birthdateData = JObject.Parse(await GetStringAsync("/v1/birthdate"));
            birthDate = new DateOnly((int)birthdateData.birthYear, (int)birthdateData.birthMonth, (int)birthdateData.birthDay);

            dynamic robuxData = JObject.Parse(await GetStringAsync("/v1/user/currency", Constants.URL("economy")));
            robux = robuxData.robux;
        }

        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("users");

        internal User? user;

        /// <summary>
        /// Gets a <see cref="API.User"/> representing the currently authenticated user.
        /// </summary>
        [UsesSession]
        public User User => user;

        private Gender gender;

        /// <summary>
        /// Gets the gender of the authenticated user.
        /// </summary>
        public Gender Gender => gender;


        private string countryCode;

        /// <summary>
        /// Gets the country code of the authenticated user.
        /// </summary>
        public string CountryCode => countryCode;


        private DateOnly birthDate;

        /// <summary>
        /// Gets a <see cref="DateOnly"/> representing the set birth date of the user.
        /// </summary>
        public DateOnly BirthDate => birthDate;


        private int robux;

        /// <summary>
        /// Gets the amount of Robux the authenticated user has.
        /// </summary>
        public int Robux => robux;

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

        private string[] incomeSkipList = ["incomingRobuxTotal", "outgoingRobuxTotal"];
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
