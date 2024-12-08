using Newtonsoft.Json.Linq;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Extensions;
using RoSharp.Interfaces;

namespace RoSharp.API.Assets.Experiences
{
    /// <summary>
    /// Represents a single location within a <see cref="Experiences.Experience"/> (universe).
    /// </summary>
    public class Place : APIMain, IRefreshable, IIdApi<Place>
    {
        /// <inheritdoc/>
        public override string BaseUrl => Constants.URL("games");

        /// <summary>
        /// Gets the unique Id of the place.
        /// </summary>
        public ulong Id { get; }

        private string name;

        /// <summary>
        /// Gets the name of this place.
        /// </summary>
        public string Name => name;

        private string description;

        /// <summary>
        /// Gets the description of this place.
        /// </summary>
        public string Description => description;

        private Experience experience;

        /// <summary>
        /// Gets the governing <see cref="Experiences.Experience"/> that this place is apart of.
        /// </summary>
        public Experience Experience => experience;

        private ulong ownerId;
        private string ownerName;
        private bool isCommunityOwned;

        /// <summary>
        /// Gets the unique Id (community or user) of the owner of this place.
        /// </summary>
        public ulong OwnerId => ownerId;

        /// <summary>
        /// Gets the name (community or user) of the owner of this place.
        /// </summary>
        public string OwnerName => ownerName;

        /// <summary>
        /// Gets whether or not this place is owned by a community.
        /// </summary>
        /// <seealso cref="GetOwnerAsync"/>
        public bool IsCommunityOwned => isCommunityOwned;

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/games/{Id}/";

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private Place(ulong id, Session? session)
        {
            Id = id;

            if (session != null)
                AttachSession(session);

            if (!RoPool<Place>.Contains(Id))
                RoPool<Place>.Add(this);
        }

        /// <summary>
        /// Gets a place from the provided Id.
        /// </summary>
        /// <param name="id">The Id of the place.</param>
        /// <param name="session"></param>
        /// <returns>A task containing the <see cref="Place"/> upon completion.</returns>
        /// <remarks>For a quicker way to get stats on an entire universe, see <see cref="Experience.FromPlaceId(ulong, Session?)"/>.</remarks>
        /// <exception cref="ArgumentException">Invalid place Id provided.</exception>
        /// <exception cref="Exceptions.RobloxAPIException">Roblox API failure.</exception>
        public static async Task<Place> FromId(ulong id, Session? session = null)
        {
            if (RoPool<Place>.Contains(id))
                return RoPool<Place>.Get(id, session.Global());

            Place newUser = new(id, session.Global());
            await newUser.RefreshAsync();

            return newUser;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentException">Invalid place Id provided.</exception>
        public async Task RefreshAsync()
        {
            HttpResponseMessage message = await SendAsync(HttpMethod.Get, $"/v1/games/multiget-place-details?placeIds={Id}");
            JArray dataUseless = JArray.Parse(await message.Content.ReadAsStringAsync());

            if (dataUseless.Count == 0)
                throw new ArgumentException("Invalid place Id provided.", "id");

            foreach (dynamic data in dataUseless.Children<JObject>())
            {
                name = data.sourceName;
                description = data.sourceDescription;

                experience = await Experiences.Experience.FromId(Convert.ToUInt64(data.universeId), session);
                ownerId = experience.OwnerId;
                ownerName = experience.OwnerName;
                isCommunityOwned = experience.IsCommunityOwned;
            }
        }

        /// <summary>
        /// Returns the owner of this asset.
        /// </summary>
        /// <returns>A task containing the owner of this asset.</returns>
        public async Task<IAssetOwner> GetOwnerAsync()
        {
            if (IsCommunityOwned)
            {
                return await Community.FromId(OwnerId);
            }
            return await User.FromId(OwnerId);
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
                };
                list.Add(server);
            }

            return new(list, nextPage, previousPage);
        }

        /// <inheritdoc/>
        public Place AttachSessionAndReturn(Session? session)
        {
            if (session is null || !session.LoggedIn)
                DetachSession();
            else
                AttachSession(session);
            return this;
        }
    }
}
