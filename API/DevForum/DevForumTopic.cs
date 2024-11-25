using System.Collections.ObjectModel;

namespace RoSharp.API.DevForum
{
    /// <summary>
    /// Represents a topic on the Roblox Developer Forum.
    /// </summary>
    public class DevForumTopic
    {
        /// <summary>
        /// Gets the unique Id of this topic.
        /// </summary>
        public ulong Id { get; init; }

        /// <summary>
        /// Gets the category Id this topic is located in.
        /// </summary>
        public ushort CategoryId { get; init; }

        /// <summary>
        /// Gets the title of this topic.
        /// </summary>
        public string Title { get; init; }

        /// <summary>
        /// Gets the total amount of views this topic has.
        /// </summary>
        public int Views { get; init; }

        /// <summary>
        /// Gets whether or not this topic is pinned in the category.
        /// </summary>
        public bool Pinned { get; init; }

        /// <summary>
        /// Gets whether or not this topic is pinned globally (shown at the top of the DevForum in a header).
        /// </summary>
        public bool PinnedGlobally { get; init; }

        /// <summary>
        /// Gets whether or not this topic is closed (no more replies can be added).
        /// </summary>
        public bool Closed { get; init; }

        /// <summary>
        /// Gets whether or not this topic is archived (no longer visible other than to staff).
        /// </summary>
        public bool Archived { get; init; }

        /// <summary>
        /// Gets whether or not this topic is visible (false = must have URL to see post).
        /// </summary>
        public bool Visible { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="string"/>s representing the tags the topic author added.
        /// </summary>
        public ReadOnlyCollection<string> Tags { get; init; }

        /// <summary>
        /// Gets a <see cref="DevForumPost"/> for the first post in this topic.
        /// </summary>
        public DevForumPost OriginalPost { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="DevForumPost"/> representing replies to the <see cref="OriginalPost"/>.
        /// </summary>
        public ReadOnlyCollection<DevForumPost> Replies { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of <see cref="string"/>s of usernames of users that have participated in this topic.
        /// </summary>
        public ReadOnlyCollection<string> Participants { get; init; }

        /// <summary>
        /// Gets a <see cref="ReadOnlyCollection{T}"/> of recommended topics.
        /// </summary>
        public ReadOnlyCollection<ulong> RecommendedTopicsIds { get; init; }
    }
}
