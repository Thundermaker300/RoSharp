using Newtonsoft.Json.Linq;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using RoSharp.Interfaces;
using RoSharp.Structures;
using System.Collections.ObjectModel;

namespace RoSharp.API
{
    // TODO: The API in this class needs converted to use the new system (SessionVerify.ThrowIfNecessary)
    // The API should also all be obtained in a ctor or refresh method instead of each access.

    /// <summary>
    /// Some API that only the authenticated user can see and perform (such as their Robux amount).
    /// </summary>
    public class SessionAPI : APIMain, IRefreshable
    {
        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private SessionAPI(Session session) : base(session) { }

        /// <summary>
        /// Gets a <see cref="SessionAPI"/> from the given <see cref="Session"/>.
        /// </summary>
        /// <param name="session">The session to use. Must be authenticated.</param>
        /// <returns>A task containing a <see cref="SessionAPI"/> upon completion.</returns>
        /// <exception cref="ArgumentException">Session cannot be null and must be authenticated.</exception>
        public static async Task<SessionAPI> FromSession(Session session)
        {
            if (!SessionVerify.Verify(session))
                throw new ArgumentException("Session cannot be null and must be authenticated.", nameof(session));

            SessionAPI api = new(session);
            await api.RefreshAsync();

            return api;
        }

        /// <inheritdoc/>
        public async Task RefreshAsync()
        {
            if (session == null)
                throw new InvalidOperationException("SessionAPI must have a session.");

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

        /// <summary>
        /// Returns the authenticated user's incoming Robux for the specified time length.
        /// </summary>
        /// <param name="timeLength">The time length of the data.</param>
        /// <returns>The data in the form of an <see cref="EconomyBreakdown"/> struct.</returns>
        public async Task<EconomyBreakdown> GetIncomeAsync(AnalyticTimeLength timeLength = AnalyticTimeLength.Day)
        {
            if (session == null)
                throw new InvalidOperationException("Session cannot be null.");

            var url = $"/v2/users/{session.userid}/transaction-totals?timeFrame={timeLength}&transactionType=summary";
            string rawData = await GetStringAsync(url, Constants.URL("economy"), verifyApiName: "SessionAPI.GetIncomeAsync");
            dynamic data = JObject.Parse(rawData);

            int amount = 0;
            int pending = 0;
            Dictionary<IncomeType, int> breakdown = [];

            foreach (dynamic cat in data)
            {
                string catName = Convert.ToString(cat.Name);

                if (incomeSkipList.Any(sky => catName.Equals(sky, StringComparison.OrdinalIgnoreCase)))
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

        public async Task<ReadOnlyCollection<PrivateMessage>> GetPrivateMessagesAsync(int pageNumber = 0, int pageSize = 20, MessagesPageTab tab = MessagesPageTab.Inbox)
        {
            string url = tab is MessagesPageTab.News
                ? "/v1/announcements"
                : $"/v1/messages?messageTab={tab.ToString().ToLower()}&pageNumber={pageNumber}&pageSize={pageSize}";
            string rawData = await GetStringAsync(url, Constants.URL("privatemessages"), "SessionAPI.ReadPrivateMessagesAsync");
            dynamic data = JObject.Parse(rawData);

            List<PrivateMessage> messages = new List<PrivateMessage>();
            foreach (dynamic item in data.collection)
            {
                PrivateMessage message = new()
                {
                    Id = item.id,
                    Subject = item.subject,
                    Text = item.body,
                    Recipient = new(Convert.ToUInt64(item.recipient.id)),
                    Sender = new(Convert.ToUInt64(item.sender.id)),
                    Created = item.created,
                    IsRead = item.isRead,
                    IsSystemMessage = item.isSystemMessage,
                    CurrentTab = tab,
                };
                messages.Add(message);
            }
            return messages.AsReadOnly();
        }

        public async Task MarkReadAsync(ulong messageId)
        {
            object body = new
            {
                messageIds = new[] { messageId },
            };

            await PostAsync("/v1/messages/mark-read", body, Constants.URL("privatemessages"), "SessionAPI.MarkReadAsync");
        }

        public async Task MarkReadAsync(PrivateMessage message)
            => await MarkReadAsync(message.Id);

        public async Task MarkUnreadAsync(ulong messageId)
        {
            object body = new
            {
                messageIds = new[] { messageId },
            };

            await PostAsync("/v1/messages/mark-unread", body, Constants.URL("privatemessages"), "SessionAPI.MarkUnreadAsync");
        }

        public async Task MarkUnreadAsync(PrivateMessage message)
            => await MarkUnreadAsync(message.Id);


        public async Task ArchiveAsync(ulong messageId)
        {
            object body = new
            {
                messageIds = new[] { messageId },
            };

            await PostAsync("/v1/messages/archive", body, Constants.URL("privatemessages"), "SessionAPI.ArchiveAsync");
        }

        public async Task ArchiveAsync(PrivateMessage message)
            => await ArchiveAsync(message.Id);

        public async Task UnarchiveAsync(ulong messageId)
        {
            object body = new
            {
                messageIds = new[] { messageId },
            };

            await PostAsync("/v1/messages/unarchive", body, Constants.URL("privatemessages"), "SessionAPI.UnarchiveAsync");
        }

        public async Task UnarchiveAsync(PrivateMessage message)
            => await UnarchiveAsync(message.Id);
    }
}
