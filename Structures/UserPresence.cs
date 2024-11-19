using RoSharp.API.Assets;
using RoSharp.Enums;

namespace RoSharp.Structures
{
    public class UserPresence
    {
        public UserLocationType Location { get; }
        public Experience? PresenceLocation { get; }
        public DateTime LastOnline { get; }
        public bool IsOnline => Location is not UserLocationType.Offline;

        internal UserPresence(UserLocationType type, Experience? location, DateTime lastOnline)
        {
            Location = type;
            PresenceLocation = location;
            LastOnline = lastOnline;
        }
    }
}
