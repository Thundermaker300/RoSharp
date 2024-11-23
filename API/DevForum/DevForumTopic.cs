using System.Collections.ObjectModel;

namespace RoSharp.API.DevForum
{
    public class DevForumTopic
    {
        public ulong Id { get; init; }
        public ushort CategoryId { get; init; }
        public string Title { get; init; }
        public int Views { get; init; }
        public bool Pinned { get; init; }
        public bool PinnedGlobally { get; init; }
        public bool Closed { get; init; }
        public bool Archived { get; init; }
        public bool Visible { get; init; }
        public ReadOnlyCollection<string> Tags { get; init; }
        public DevForumPost OriginalPost { get; init; }
        public ReadOnlyCollection<DevForumPost> Replies { get; init; }
        public ReadOnlyCollection<string> Participants { get; init; }
        public ReadOnlyCollection<ulong> RecommendedTopicsIds { get; init; }
    }
}
