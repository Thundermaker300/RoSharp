using RoSharp.API.Assets;
using RoSharp.API.Assets.Experiences;
using RoSharp.Enums;

namespace RoSharp.Structures
{
    /// <summary>
    /// Represents a user's current state.
    /// </summary>
    public class UserPresence
    {
        /// <summary>
        /// Gets their current location.
        /// </summary>
        public UserLocationType Location { get; }

        /// <summary>
        /// If <see cref="Location"/> is <see cref="UserLocationType.Experience"/>, gets the Experience they are playing.
        /// </summary>
        public Experience? PresenceLocation { get; }
        
        /// <summary>
        /// Gets when they were last seen online.
        /// </summary>
        public DateTime LastOnline { get; }

        /// <summary>
        /// Gets whether or not they are currently online (Website, Experience or Studio).
        /// </summary>
        public bool IsOnline => Location is not UserLocationType.Offline;

        internal UserPresence(UserLocationType type, Experience? location, DateTime lastOnline)
        {
            Location = type;
            PresenceLocation = location;
            LastOnline = lastOnline;
        }
    }
}
