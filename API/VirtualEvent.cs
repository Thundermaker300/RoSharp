using Newtonsoft.Json.Linq;
using RoSharp.API.Assets;
using RoSharp.API.Assets.Experiences;
using RoSharp.API.Communities;
using RoSharp.API.Pooling;
using RoSharp.Enums;
using RoSharp.Extensions;
using RoSharp.Interfaces;

namespace RoSharp.API
{
    public class VirtualEvent : APIMain, IRefreshable, IIdApi<VirtualEvent>
    {

        /// <inheritdoc/>
        public ulong Id { get; }

        /// <inheritdoc/>
        public string Url => $"{Constants.ROBLOX_URL}/events/{Id}";

        private string title;

        public string Title => title;

        private string subtitle;

        public string Subtitle => subtitle;

        private string description;

        public string Description => description;

        private DateTime startTime;

        public DateTime StartTime => startTime;


        private DateTime endTime;
        public DateTime EndTime => endTime;

        /// <summary>
        /// Gets a <see cref="TimeSpan"/> representing the length of the event. Equivalent to <c>EndTime - StartTime</c>.
        /// </summary>
        public TimeSpan Length => endTime - startTime;

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
        public ulong HostId => hostId;

        private bool isCommunityHosted;
        public bool IsCommunityHosted => isCommunityHosted;

        private Id<Experience> experience;
        public Id<Experience> Experience => experience;

        private Id<Place> place;
        public Id<Place> Place => place;

        private VirtualEventStatus status;
        public VirtualEventStatus Status => status;

        private string visibility;
        public string Visibility => visibility;

        private DateTime created;
        public DateTime Created => created;

        private DateTime updated;
        public DateTime Updated => updated;

        private VirtualEventCategory category;
        public VirtualEventCategory Category => category;

        private Id<Asset> thumbnail;
        public Id<Asset> Thumbnail => thumbnail;

        private int totalRSVPs;

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
            visibility = data.eventVisibility;
            created = data.createdUtc;
            updated = data.updatedUtc;
            category = Enum.Parse<VirtualEventCategory>(Convert.ToString(data.eventCategories[0].category), true);

            if (data.thumbnails != null)
                thumbnail = new(Convert.ToUInt64(data.thumbnails[0].mediaId), session);

            string rawRsvps = await SendStringAsync(HttpMethod.Get, $"{Constants.URL("apis")}/virtual-events/v1/virtual-events/{Id}/rsvps/counters");
            dynamic rsvps = JObject.Parse(rawRsvps);
            totalRSVPs = rsvps.counters.going;
        }
        public async Task<PageResponse<Id<User>>> GetRSVPsAsync(string? cursor = null)
        {
            string url = $"/virtual-events/v1/virtual-events/{Id}/rsvps";
            if (cursor != null)
                url += $"?cursor={cursor}";

            string rawData = await SendStringAsync(HttpMethod.Get, url, Constants.URL("apis"));
            dynamic data = JObject.Parse(rawData);
            string? nextPage = data.nextPageCursor;
            string? previousPage = data.previousPageCursor;

            List<Id<User>> rsvps = [];
            foreach (dynamic comment in data.data)
            {
                if (comment.rsvpStatus != "going") continue;
                rsvps.Add(new(Convert.ToUInt64(comment.userId), session));
            }

            return new PageResponse<Id<User>>(rsvps, nextPage, previousPage);
        }

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

        public override string ToString()
        {
            return $"[{Id}] '{Title}' {Subtitle} [{Category}] [From: {StartTime}] [To: {EndTime}] <{Status}> |{Visibility}|";
        }
    }
}
