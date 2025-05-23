using Newtonsoft.Json.Linq;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Interfaces;
using RoSharp.Structures;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// Represents a single location within a <see cref="Experiences.Experience"/> (universe).
    /// </summary>
    public class Place : Asset, IRefreshable, IIdApi<Place>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("games");

        private Experience experience;

        /// <summary>
        /// Gets the governing <see cref="Experiences.Experience"/> that this place is apart of.
        /// </summary>
        public Experience Experience => experience;

        /// <inheritdoc/>
        public new string Url => $"{Constants.ROBLOX_URL}/games/{Id}/";

        /// <inheritdoc cref="Experience.Favorites"/>
        public new ulong Favorites => Experience.Favorites;

        private Place(ulong id, Session? session)
            : base(id, session) { }

        /// <summary>
        /// Gets a place from the provided Id.
        /// </summary>
        /// <param name="id">The Id of the place.</param>
        /// <param name="session"></param>
        /// <returns>A task containing the <see cref="Place"/> upon completion.</returns>
        /// <remarks>For a quicker way to get stats on an entire universe, see <see cref="Experience.FromPlaceId(ulong, Session?)"/>.</remarks>
        /// <exception cref="ArgumentException">Invalid place Id provided.</exception>
        /// <exception cref="Exceptions.RobloxAPIException">Roblox API failure.</exception>
        public new static async Task<Place> FromId(ulong id, Session? session = null)
        {
            if (RoPool<Place>.Contains(id))
                return RoPool<Place>.Get(id, session.Global());

            Place newUser = new(id, session.Global());
            await newUser.RefreshAsync();

            RoPool<Place>.Add(newUser);

            return newUser;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Invalid place Id provided.</exception>
        public new async Task RefreshAsync()
        {
            await base.RefreshAsync();
            HttpResponseMessage message = await SendAsync(HttpMethod.Get, $"/v1/games/multiget-place-details?placeIds={Id}");
            JArray dataUseless = JArray.Parse(await message.Content.ReadAsStringAsync());

            if (dataUseless.Count == 0)
                throw new ArgumentException("Invalid place Id provided.", "id");

            foreach (dynamic data in dataUseless.Children<JObject>())
            {
                experience = await Experiences.Experience.FromId(Convert.ToUInt64(data.universeId), session);
            }
        }

        /// <summary>
        /// Gets this experience's icon.
        /// </summary>
        /// <returns>A task containing the string URL for the icon upon completion.</returns>
        /// <exception cref="ArgumentException">Invalid asset to get thumbnail for.</exception>
        public async Task<string> GetIconAsync(IconSize size = IconSize.S420x420)
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"/v1/places/gameicons?placeIds={Id}&returnPolicy=PlaceHolder&size={size.ToString().Substring(1)}&format=Png&isCircular=false", Constants.URL("thumbnails"));
            dynamic data = JObject.Parse(rawData);

            if (data.data.Count == 0)
                throw new ArgumentException("Invalid asset to get thumbnail for.");
            return data.data[0].imageUrl;
        }

        /// <summary>
        /// Gets live servers in this place.
        /// </summary>
        /// <param name="limit">The maximum amount of servers to return.</param>
        /// <param name="sortOrder">Sort order. <c>1</c> = Least to most players, <c>2</c> = Most to least players. Any other input will be treated as <c>2</c>.</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns></returns>
        public async Task<PageResponse<GameServer>> GetLiveServersAsync(FixedLimit limit = FixedLimit.Limit50, int sortOrder = 2, string? cursor = null)
        {
            string url = $"/v1/games/{Id}/servers/0?sortOrder={sortOrder}&excludeFullGames=false&limit={limit.Limit()}";
            if (cursor != null)
                url += $"&cursor={cursor}";

            string rawData = await SendStringAsync(HttpMethod.Get, url);
            dynamic data = JObject.Parse(rawData);

            List<GameServer> list = [];
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            foreach (dynamic item in data.data)
            {
                GameServer server = new()
                {
                    ServerId = item.id,
                    MaxPlayers = item.maxPlayers,
                    Playing = item.playing,
                    AverageFps = item.fps,
                    AveragePing = item.ping,

                    place = this,
                };
                list.Add(server);
            }

            return new(list, nextPage, previousPage);
        }

        /// <summary>
        /// Creates a virtual event.
        /// </summary>
        /// <param name="settings">The settings for the virtual event.</param>
        /// <returns>A task that contains a <see cref="VirtualEvent"/> upon completion.</returns>
        /// <exception cref="InvalidOperationException">Missing any of the following properties: Title, Subtitle, Description, Category, Visibility, StartTime, EndTime.</exception>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<VirtualEvent> CreateVirtualEventAsync(VirtualEventConfiguration settings)
        {
            if (settings.Title == null || settings.Subtitle == null || settings.Description == null) throw new InvalidOperationException("Title, Subtitle, and Description cannot be null!");
            if (!settings.Category.HasValue || !settings.Visibility.HasValue) throw new InvalidOperationException("Category and Visibility must be provided!");
            if (!settings.StartTime.HasValue || !settings.EndTime.HasValue)
                throw new InvalidOperationException("StartTime and EndTime must be provided!");

            string formattedCategory = settings.Category.Value.ToString().Substring(0, 1).ToLower() + settings.Category.Value.ToString().Substring(1);
            ulong? groupid = (Experience.IsCommunityOwned ? Experience.OwnerId : null);
            object body = new
            {
                title = settings.Title,
                subtitle = settings.Subtitle,
                description = settings.Description,
                placeId = Id,
                universeId = Experience.Id,
                groupId = groupid,
                eventTime = new { startTime = settings.StartTime.Value.ToString("s") + ".000Z", endTime = settings.EndTime.Value.ToString("s") + ".000Z" },
                eventCategories = new[] { new { category = formattedCategory, rank = 0 } },
                visibility = settings.Visibility.Value.ToString().ToLower(),
            };
            HttpMessage payload = new(HttpMethod.Post, $"/virtual-events/v1/virtual-events/create", body)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(CreateVirtualEventAsync)
            };
            HttpResponseMessage response = await SendAsync(payload, Constants.URL("apis"));
            string responseData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(responseData);
            return await VirtualEvent.FromId(Convert.ToUInt64(data.id), session);
        }

        /// <summary>
        /// Gets feedback provided for this place.
        /// </summary>
        /// <param name="startTime">The start time for the feedback search.</param>
        /// <param name="endTime">The end time for the feedback search.</param>
        /// <param name="limit">The limit of feedback to return.</param>
        /// <param name="voteTypeFilter">Whether to vote by positive (<see langword="true"/>) feedback, negative (<see langword="false"/>) feedback, or both (<see langword="null"/>).</param>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="ExperienceReview"/> upon completion.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<PageResponse<ExperienceReview>> GetFeedbackAsync(DateTime? startTime = null, DateTime? endTime = null, FixedLimit limit = FixedLimit.Limit50, bool? voteTypeFilter = null, string? cursor = null)
        {
            string url = $"/player-generated-reviews-service/v1/channels/experience-discovery-page/assets/{Id}/reviews?limit={limit.Limit()}&hasComment=false";
            if (startTime.HasValue)
            {
                url += $"&startTime={startTime.Value.ToString("s")}Z";
            }
            if (endTime.HasValue)
            {
                url += $"&endTime={endTime.Value.ToString("s")}Z";
            }
            if (voteTypeFilter.HasValue)
            {
                url += $"&categoryType={(voteTypeFilter.Value == true ? "Upvote" : "Downvote")}";
            }

            HttpMessage message = new(HttpMethod.Get, url)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(GetFeedbackAsync)
            };

            string rawData = await SendStringAsync(message, Constants.URL("apis"));
            dynamic data = JObject.Parse(rawData);

            List<ExperienceReview> list = [];
            foreach (dynamic review in data.reviews)
            {
                list.Add(new()
                {
                    Id = review.id,
                    TargetExperience = new Id<Experience>(Convert.ToUInt64(review.asset_id), session),
                    TargetVersion = review.asset_version,
                    Comment = review.comment,
                    Positive = review.category_type == "Upvote",
                    Created = DateTime.SpecifyKind(DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(review.created_utc)).DateTime, DateTimeKind.Utc),
                    DeviceType = Enum.Parse<Device>(Convert.ToString(review.metadata.device_type)),
                    DeviceOS = review.metadata.operating_system_type,
                });
            }

            return new(list, data.has_more == true ? Convert.ToString(data.next_cursor) : null, null);
        }

        /// <inheritdoc/>
        public new Place AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Place {Name} [{Id}] {{EXP:{experience.Id}}}";
        }
    }

    /// <summary>
    /// Configuration for a virtual event.
    /// </summary>
    public sealed class VirtualEventConfiguration
    {
        /// <inheritdoc cref="VirtualEvent.Title"/>
        public string? Title { get; set; }

        /// <inheritdoc cref="VirtualEvent.Subtitle"/>
        public string? Subtitle { get; set; }

        /// <inheritdoc cref="VirtualEvent.Description"/>
        public string? Description { get; set; }

        /// <inheritdoc cref="VirtualEvent.StartTime"/>
        public DateTime? StartTime { get; set; }

        /// <inheritdoc cref="VirtualEvent.EndTime"/>
        public DateTime? EndTime { get; set; }

        /// <inheritdoc cref="VirtualEvent.Category"/>
        public VirtualEventCategory? Category { get; set; }

        /// <inheritdoc cref="VirtualEvent.Visibility"/>
        public VirtualEventVisibility? Visibility { get; set; }
    }
}
