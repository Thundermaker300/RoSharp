using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;
using System.Xml.Linq;

namespace RoSharp.API.Communities
{
    /// <summary>
    /// Represents a single community blocked word.
    /// </summary>
    public readonly struct CommunityBlockedWord
    {
        /// <summary>
        /// Gets the unique Id of the blocked keyword.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Gets the keyword that is blocked.
        /// </summary>
        public string Keyword { get; init; }

        /// <summary>
        /// Gets whether or not the keyword is private.
        /// </summary> // TODO FIGURE OUT  WHAT THIS IS
        public bool IsPrivate { get; init; }

        /// <summary>
        /// The time the blocked word was added.
        /// </summary>
        public DateTime Created { get; init; }

        /// <summary>
        /// The time the blocked word was modified.
        /// </summary>
        public DateTime Updated { get; init; }

        /// <summary>
        /// Gets whether or not the keyword has been edited.
        /// </summary>
        public bool Edited => Created.Ticks != Updated.Ticks;

        /// <summary>
        /// The user Id of the user that added the blocked word.
        /// </summary>
        public Id<User> UserId { get; init; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"CommunityBlockedWord {Keyword} [{Id}]";
        }

    }
}
