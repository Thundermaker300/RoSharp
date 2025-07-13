using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Exceptions;
using RoSharp.Extensions;
using RoSharp.Http;
using RoSharp.Interfaces;
using RoSharp.Structures;

namespace RoSharp.API
{
    /// <summary>
    /// Represents a virtual event for an experience on Roblox.
    /// </summary>
    public class VirtualEvent : APIMain, IRefreshable, IIdApi<VirtualEvent>
    {

        /// <inheritdoc/>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/events/{Id}";

        private string title;

        /// <summary>
        /// Gets the title of the event.
        /// </summary>
        public string Title => title;

        private string subtitle;

        /// <summary>
        /// Gets the subtitle of the event.
        /// </summary>
        public string Subtitle => subtitle;

        private string description;

        /// <summary>
        /// Gets the description of the event.
        /// </summary>
        public string Description => description;

        private DateTime startTime;

        /// <summary>
        /// Gets the <see cref="DateTime"/> that the event will be starting.
        /// </summary>
        public DateTime StartTime => startTime;


        private DateTime endTime;

        /// <summary>
        /// Gets the <see cref="DateTime"/> that the event will be ending.
        /// </summary>
        public DateTime EndTime => endTime;

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> representing the length of the event. Equivalent to <c>EndTime - StartTime</c>.
        /// </summary>
        public TimeSpan Length => endTime - startTime;

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> representing the amount of time until the event starts. Equivalent to <c>StartTime - DateTime.Now</c>. Will be <c>0</c> if the event has started already.
        /// </summary>
        public TimeSpan TimeUntilStart
        {
            get
            {
                if (DateTime.Now >= StartTime)
                    return TimeSpan.Zero;

                return StartTime - DateTime.Now;
            }
        }

        private ulong hostId;

        /// <summary>
        /// Gets the Id of the host (user or community).
        /// </summary>
        public ulong HostId => hostId;

        private bool isCommunityHosted;

        /// <summary>
        /// Gets whether or not the experience hosting the event is owned by a group.
        /// </summary>
        public bool IsCommunityHosted => isCommunityHosted;

        private Id<Experience> experience;

        /// <summary>
        /// Gets the <see cref="Id{T}"/> of the experience for the event.
        /// </summary>
        public Id<Experience> Experience => experience;

        private Id<Place> place;

        /// <summary>
        /// Gets the <see cref="Id{T}"/> of the place for the event.
        /// </summary>
        public Id<Place> Place => place;

        private VirtualEventStatus status;

        /// <summary>
        /// Gets the current status of the event.
        /// </summary>
        public VirtualEventStatus Status => status;

        private VirtualEventVisibility visibility;

        /// <summary>
        /// Gets the current visibility of the event.
        /// </summary>
        public VirtualEventVisibility Visibility => visibility;

        private DateTime created;

        /// <summary>
        /// Gets the <see cref="DateTime"/> the event was created.
        /// </summary>
        public DateTime Created => created;

        private DateTime updated;

        /// <summary>
        /// Gets the <see cref="DateTime"/> the event was last updated.
        /// </summary>
        public DateTime Updated => updated;

        /// <summary>
        /// Gets whether or not this virtual event is less than 3 days old.
        /// </summary>
        public bool IsNew => (DateTime.UtcNow - Created) < TimeSpan.FromDays(3);

        private VirtualEventCategory category;

        /// <summary>
        /// Gets the category of the event.
        /// </summary>
        public VirtualEventCategory Category => category;

        private Id<Asset> thumbnail;

        /// <summary>
        /// Gets the the <see cref="Id{T}"/> of the <see cref="Asset"/> representing the event's thumbnail.
        /// </summary>
        public Id<Asset> Thumbnail => thumbnail;

        private int totalRSVPs;

        /// <summary>
        /// Gets the total amount of confirmed RSVPs for the event.
        /// </summary>
        public int TotalRSVPs => totalRSVPs;

        /// <inheritdoc/>
        public DateTime RefreshedAt { get; set; }

        private VirtualEvent(ulong userId, Session? session = null)
        {
            Id = userId;

            if (session != null)
                AttachSession(session);

            if (!RoPool<VirtualEvent>.Contains(Id))
                RoPool<VirtualEvent>.Add(this);
        }

        /// <inheritdoc/>
        public static async Task<VirtualEvent> FromId(ulong id, Session? session)
        {
            if (RoPool<VirtualEvent>.Contains(id))
                return RoPool<VirtualEvent>.Get(id, session.Global());

            VirtualEvent newEvent = new(id, session.Global());
            await newEvent.RefreshAsync();

            return newEvent;
        }

        ///<inheritdoc/>
        public async Task RefreshAsync()
        {
            string rawData = await SendStringAsync(HttpMethod.Get, $"{Constants.URL("apis")}/virtual-events/v1/virtual-events/{Id}");

            dynamic data = JObject.Parse(rawData);
            title = data.displayTitle;
            subtitle = data.displaySubtitle;
            description = data.displayDescription;
            startTime = data.eventTime.startUtc;
            endTime = data.eventTime.endUtc;
            hostId = data.host.hostId;
            isCommunityHosted = data.host.hostType == "group";
            experience = new(Convert.ToUInt64(data.universeId), session);
            place = new(Convert.ToUInt64(data.placeId), session);
            status = Enum.Parse<VirtualEventStatus>(Convert.ToString(data.eventStatus), true);
            visibility = Enum.Parse<VirtualEventVisibility>(Convert.ToString(data.eventVisibility), true);
            created = data.createdUtc;
            updated = data.updatedUtc;
            category = Enum.Parse<VirtualEventCategory>(Convert.ToString(data.eventCategories[0].category), true);

            if (data.thumbnails != null)
                thumbnail = new(Convert.ToUInt64(data.thumbnails[0].mediaId), session);

            string rawRsvps = await SendStringAsync(HttpMethod.Get, $"{Constants.URL("apis")}/virtual-events/v1/virtual-events/{Id}/rsvps/counters");
            dynamic rsvps = JObject.Parse(rawRsvps);
            totalRSVPs = rsvps.counters.going;
        }

        /// <summary>
        /// Gets this event's RSVPs.
        /// </summary>
        /// <param name="cursor">The cursor for the next page. Obtained by calling this API previously.</param>
        /// <returns>A task containing a <see cref="PageResponse{T}"/> of <see cref="Id{T}"/> upon completion.</returns>
        /// <remarks>This API method does not cache and will make a request each time it is called.</remarks>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult<PageResponse<Id<User>>>> GetRSVPsAsync(string? cursor = null)
        {
            string url = $"/virtual-events/v1/virtual-events/{Id}/rsvps";
            if (cursor != null)
                url += $"?cursor={cursor}";

            var response = await SendAsync(HttpMethod.Get, url, Constants.URL("apis"));
            string rawData = await response.Content.ReadAsStringAsync();
            dynamic data = JObject.Parse(rawData);
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            List<Id<User>> rsvps = [];
            foreach (dynamic comment in data.data)
            {
                if (comment.rsvpStatus != "going") continue;
                rsvps.Add(new(Convert.ToUInt64(comment.userId), session));
            }

            return new(response, new PageResponse<Id<User>>(rsvps, nextPage, previousPage));
        }

        /// <summary>
        /// Modifies a virtual event. Any properties not modified in the <see cref="VirtualEventConfiguration"/> class will not be modified on the site.
        /// </summary>
        /// <param name="settings">The new settings for the event.</param>
        /// <returns>A task that completes when the operation has finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> ModifyAsync(VirtualEventConfiguration settings)
        {
            string formattedCategory = (settings.Category.HasValue ? settings.Category.Value : Category).ToString().Substring(0, 1).ToLower() + (settings.Category.HasValue ? settings.Category.Value : Category).ToString().Substring(1);
            object body = new
            {
                title = settings.Title ?? Title,
                subtitle = settings.Subtitle ?? Subtitle,
                description = settings.Description ?? Description,
                eventTime = new { startTime = (settings.StartTime.HasValue ? settings.StartTime.Value : StartTime).ToString("s") + ".000Z", endTime = (settings.EndTime.HasValue ? settings.EndTime.Value : StartTime).ToString("s") + ".000Z" },
                eventCategories = new[] { new { category = formattedCategory, rank = 0 } },
                visibility = (settings.Visibility.HasValue ? settings.Visibility.Value : Visibility).ToString().ToLower(),
            };
            var payload = new HttpMessage(HttpMethod.Patch, $"/virtual-events/v1/virtual-events/{Id}", body)
            {
                AuthType = AuthType.RobloSecurity,
                ApiName = nameof(ModifyAsync),
            };
            return new(await SendAsync(payload, Constants.URL("apis")));
        }

        /// <summary>
        /// Delete this virtual event.
        /// </summary>
        /// <returns>A task that completes when the operation is finished.</returns>
        /// <exception cref="RobloxAPIException">Roblox API failure or lack of permissions.</exception>
        public async Task<HttpResult> DeleteAsync() =>
            new(await SendAsync(HttpMethod.Delete, $"/virtual-events/v1/virtual-events/{Id}", Constants.URL("apis")));

        /// <summary>
        /// Returns the host of this event.
        /// </summary>
        /// <returns>A task containing the host of this event.</returns>
        public async Task<IAssetOwner> GetHostAsync()
        {
            if (IsCommunityHosted)
            {
                return await Community.FromId(HostId);
            }
            return await User.FromId(HostId);
        }

        /// <inheritdoc/>
        public VirtualEvent AttachSessionAndReturn(Session? session)
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
            return $"VirtualEvent [{Id}] '{Title}' {Subtitle} [{Category}] [From: {StartTime}] [To: {EndTime}] <{Status}> |{Visibility}|";
        }
    }
}
